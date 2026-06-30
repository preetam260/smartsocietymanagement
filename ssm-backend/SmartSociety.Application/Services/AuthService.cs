using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;

    public AuthService(IUnitOfWork uow, IConfiguration config, IEmailService emailService)
    {
        _uow = uow;
        _config = config;
        _emailService = emailService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email);
        if(user == null || user.IsDeleted)
            throw new UnauthorizedAccessException("Invalid credentials");
        
        if(!user.IsActive) 
            throw new UnauthorizedAccessException("Account inactive");

        if(string.IsNullOrEmpty(user.PasswordHash))
            throw new UnauthorizedAccessException("Password not set. Please use forgot-password to set your password.");
        
        if(!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) 
            throw new UnauthorizedAccessException("Password Incorrect");

        var token = GenerateToken(user);

        return new LoginResponseDto
        {
            Token = token,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
        };

    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _uow.Users.GetByEmailAsync(email);
        if (user == null || user.IsDeleted || !user.IsActive)
            return;

        var token = Guid.NewGuid().ToString("N"); 
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);
        user.UpdatedAt = DateTime.UtcNow;

        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        await _emailService.SendAsync(
            user.Email,
            "SmartSociety — Password Reset",
            $@"
            Hello {user.Name},

            You requested a password reset for your SmartSociety account.

            Your reset token: {token}

            Use this token with POST /api/Auth/reset-password to set your new password.
            This token expires in 24 hours.

            If you did not request this, please ignore this email.

            Regards,
            SmartSociety Team
        ");
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid reset request.");

        if (string.IsNullOrEmpty(user.PasswordResetToken)
            || user.PasswordResetToken != dto.Token
            || user.PasswordResetTokenExpiry == null
            || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired reset token.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();
    }

    private string GenerateToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");

        var secret = jwtSettings["Key"];

        if (string.IsNullOrWhiteSpace(secret))
            throw new Exception("JWT Secret is NULL");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secret)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(
                double.Parse(jwtSettings["ExpiryHours"]!)
            ),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}