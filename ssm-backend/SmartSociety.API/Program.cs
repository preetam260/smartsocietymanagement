using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using SmartSociety.API.BackgroundJobs;
using SmartSociety.API.Hubs;
using SmartSociety.API.Middleware;
using SmartSociety.API.Services;
using SmartSociety.Application.Complaints.Triage;
using SmartSociety.Application.Interfaces;
using SmartSociety.Application.Services;
using SmartSociety.Infrastructure.Ai;
using SmartSociety.Infrastructure.BackgroundJobs;
using SmartSociety.Repository.Ai;
using SmartSociety.Repository.Context;
using SmartSociety.Repository.Interfaces;
using SmartSociety.Repository.Repositories;

var builder = WebApplication.CreateBuilder(args);

#region Serilog

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

#endregion

#region Database

builder.Services.AddDbContext<SmartSocietyDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region Repositories and Unit of Work

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IResidentRepository, ResidentRepository>();
builder.Services.AddScoped<IApartmentRepository, ApartmentRepository>();
builder.Services.AddScoped<IVisitorRepository, VisitorRepository>();
builder.Services.AddScoped<IVisitorEntryRepository, VisitorEntryRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped<IFacilityRepository, FacilityRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

#endregion

#region Application Services

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IApartmentService, ApartmentService>();
builder.Services.AddScoped<IResidentService, ResidentService>();
builder.Services.AddScoped<IVisitorService, VisitorService>();
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<BillService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IQRService, QRService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();

builder.Services.Configure<AnthropicOptions>(builder.Configuration.GetSection("Anthropic"));

builder.Services.AddHttpClient<IComplaintTriageService, ComplaintTriageService>();

builder.Services.AddSingleton<IComplaintTriageQueue, ComplaintTriageQueue>();


#endregion

#region SignalR

builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationPushService, NotificationPushService>();

#endregion

#region Background Services

builder.Services.AddHostedService<BillingBackgroundService>();
builder.Services.AddHostedService<BookingHoldExpirationService>();
builder.Services.AddHostedService<ComplaintTriageBackgroundService>();

#endregion

#region JWT Authentication

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken =
                context.Request.Query["access_token"];

            var path =
                context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

#endregion

#region Rate Limiting

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromSeconds(30);
        limiterOptions.QueueProcessingOrder =
            QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("global", limiterOptions =>
    {
        limiterOptions.PermitLimit = 200;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder =
            QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;
});

#endregion

#region Controllers and Swagger

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // ADD THIS
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartSociety API",
        Version = "v1",
        Description = "API for SmartSociety Management System"
    });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description =
                "Please enter your JWT token without the Bearer prefix."
        });

    options.AddSecurityRequirement(
        doc => new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference("Bearer", doc)
            ] = new List<string>()
        });
});

#endregion

#region CORS

var corsOrigins = builder.Configuration["CorsOrigins"]?.Split(',')
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

#endregion

#region Application Pipeline

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseCors("AllowAngular");

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notifications");

#endregion

#region Application Startup

try
{
    Log.Information("Starting Smart Society Management API");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(
        ex,
        "Smart Society Management API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

#endregion
