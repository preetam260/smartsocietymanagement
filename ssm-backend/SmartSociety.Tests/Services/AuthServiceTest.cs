using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Services;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;
using SmartSociety.Domain.Enums;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class AuthServiceTest
{
    private Mock<IUnitOfWork> _mockUow;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<IResidentRepository> _mockResidentRepo;
    private Mock<IConfiguration> _mockConfig;
    private Mock<IEmailService> _mockEmail;
    private AuthService _service;

    [SetUp]
    public void Setup()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockResidentRepo = new Mock<IResidentRepository>();
        _mockConfig = new Mock<IConfiguration>();
        _mockEmail = new Mock<IEmailService>();

        _mockUow.Setup(u => u.Users).Returns(_mockUserRepo.Object);
        _mockUow.Setup(u => u.Residents).Returns(_mockResidentRepo.Object);

        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s["Key"]).Returns("SuperSecretTestKey_AtLeast32CharsLong!!");
        mockSection.Setup(s => s["Issuer"]).Returns("SmartSociety");
        mockSection.Setup(s => s["Audience"]).Returns("SmartSocietyUsers");
        mockSection.Setup(s => s["ExpiryHours"]).Returns("24");

        _mockConfig.Setup(c => c.GetSection("JwtSettings")).Returns(mockSection.Object);

        _service = new AuthService(_mockUow.Object, _mockConfig.Object, _mockEmail.Object);
    }

    [Test]
    public void LoginAsync_ThrowsUnauthorized_WhenUserNotFound()
    {
        var dto = new LoginRequestDto { Email = "missing@test.com", Password = "Password123!" };
        _mockUserRepo.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync((User)null!);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.LoginAsync(dto));
    }

    [Test]
    public void LoginAsync_ThrowsUnauthorized_WhenUserInactive()
    {
        var dto = new LoginRequestDto { Email = "inactive@test.com", Password = "Password123!" };
        var user = new User { Email = "inactive@test.com", IsActive = false };
        _mockUserRepo.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync(user);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.LoginAsync(dto));
    }

    [Test]
    public void LoginAsync_ThrowsUnauthorized_WhenPasswordNotSet()
    {
        var dto = new LoginRequestDto { Email = "notset@test.com", Password = "Password123!" };
        var user = new User { Email = "notset@test.com", IsActive = true, PasswordHash = "" };
        _mockUserRepo.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync(user);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.LoginAsync(dto));
    }

    [Test]
    public void LoginAsync_ThrowsUnauthorized_WhenPasswordIncorrect()
    {
        var dto = new LoginRequestDto { Email = "test@test.com", Password = "WrongPassword!" };
        var hash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!");
        var user = new User { Email = "test@test.com", IsActive = true, PasswordHash = hash };
        _mockUserRepo.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync(user);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.LoginAsync(dto));
    }

    [Test]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsValid()
    {
        var dto = new LoginRequestDto { Email = "test@test.com", Password = "CorrectPassword123!" };
        var hash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!");
        var user = new User 
        { 
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@test.com", 
            IsActive = true, 
            PasswordHash = hash,
            Role = UserRole.Resident
        };
        _mockUserRepo.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync(user);

        var result = await _service.LoginAsync(dto);

        Assert.That(result.Email, Is.EqualTo(dto.Email));
        Assert.That(result.Name, Is.EqualTo(user.Name));
        Assert.That(result.Token, Is.Not.Null);
    }

    [Test]
    public async Task ForgotPasswordAsync_SilentReturn_WhenUserNotFoundOrInactive()
    {
        _mockUserRepo.Setup(u => u.GetByEmailAsync("missing@test.com")).ReturnsAsync((User)null!);

        await _service.ForgotPasswordAsync("missing@test.com");

        _mockUserRepo.Verify(u => u.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task ForgotPasswordAsync_SendsResetEmail_WhenUserValid()
    {
        var user = new User { Name = "User", Email = "test@test.com", IsActive = true };
        _mockUserRepo.Setup(u => u.GetByEmailAsync("test@test.com")).ReturnsAsync(user);

        await _service.ForgotPasswordAsync("test@test.com");

        Assert.That(user.PasswordResetToken, Is.Not.Null);
        Assert.That(user.PasswordResetTokenExpiry, Is.GreaterThan(DateTime.UtcNow));
        _mockUserRepo.Verify(u => u.UpdateAsync(user), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockEmail.Verify(e => e.SendAsync(user.Email, It.IsAny<string>(), It.IsAny<string>(), null, null), Times.Once);
    }

    [Test]
    public void ResetPasswordAsync_ThrowsUnauthorized_WhenUserNotFound()
    {
        var dto = new ResetPasswordDto { Email = "missing@test.com", Token = "tok", NewPassword = "NewPass!" };
        _mockUserRepo.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync((User)null!);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.ResetPasswordAsync(dto));
    }

    [Test]
    public void ResetPasswordAsync_ThrowsUnauthorized_WhenTokenExpiredOrInvalid()
    {
        var dto = new ResetPasswordDto { Email = "test@test.com", Token = "tok", NewPassword = "NewPass!" };
        var user = new User { Email = "test@test.com", PasswordResetToken = "different_tok", PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1) };
        _mockUserRepo.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync(user);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.ResetPasswordAsync(dto));

        user.PasswordResetToken = "tok";
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(-5); // Expired

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.ResetPasswordAsync(dto));
    }

    [Test]
    public async Task ResetPasswordAsync_UpdatesPassword_WhenTokenValid()
    {
        var dto = new ResetPasswordDto { Email = "test@test.com", Token = "tok", NewPassword = "NewPass!" };
        var user = new User { Email = "test@test.com", PasswordResetToken = "tok", PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1) };
        _mockUserRepo.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync(user);

        await _service.ResetPasswordAsync(dto);

        Assert.That(user.PasswordResetToken, Is.Null);
        Assert.That(user.PasswordResetTokenExpiry, Is.Null);
        Assert.That(BCrypt.Net.BCrypt.Verify("NewPass!", user.PasswordHash), Is.True);
        _mockUserRepo.Verify(u => u.UpdateAsync(user), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
