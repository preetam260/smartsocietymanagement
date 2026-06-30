using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SmartSociety.API.Middleware;
using SmartSociety.API.BackgroundJobs;
using SmartSociety.Application.Interfaces;
using SmartSociety.Application.Services;
using SmartSociety.Repository.Context;
using SmartSociety.Repository.Interfaces;
using SmartSociety.Repository.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────
//  DATABASE
// ─────────────────────────────────────────────────────────

builder.Services.AddDbContext<SmartSocietyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─────────────────────────────────────────────────────────
//  REPOSITORIES + UNIT OF WORK
// ─────────────────────────────────────────────────────────

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

// ─────────────────────────────────────────────────────────
//  APPLICATION SERVICES
// ─────────────────────────────────────────────────────────

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IApartmentService, ApartmentService>();
builder.Services.AddScoped<IResidentService, ResidentService>();
builder.Services.AddScoped<IVisitorService, VisitorService>();
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<BillService>(); // PaymentService depends on concrete type
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IQRService, QRService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();

// ─────────────────────────────────────────────────────────
//  BACKGROUND SERVICES
// ─────────────────────────────────────────────────────────

builder.Services.AddHostedService<BillingBackgroundService>();

// ─────────────────────────────────────────────────────────
//  JWT AUTHENTICATION
// ─────────────────────────────────────────────────────────

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
});

builder.Services.AddAuthorization();

// ─────────────────────────────────────────────────────────
//  SWAGGER + CONTROLLERS
// ─────────────────────────────────────────────────────────

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
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

    // JWT Bearer auth definition for Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Please enter your JWT token (without the 'Bearer ' prefix)."
    });

    // Apply it globally so every endpoint gets the lock icon
    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", doc)] = new List<string>()
    });
});

// ─────────────────────────────────────────────────────────
//  CORS
// ─────────────────────────────────────────────────────────

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ─────────────────────────────────────────────────────────
//  BUILD + PIPELINE
// ─────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

// Global exception handler — must be before auth/controllers
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();