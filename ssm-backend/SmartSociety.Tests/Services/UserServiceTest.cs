using Moq;
using NUnit.Framework;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Services;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class UserServiceTest
{
    private Mock<IUnitOfWork> _mockUow;
    private Mock<IUserRepository> _mockRepo;
    private Mock<IEmailService> _mockEmail;
    private UserService _service;

    [SetUp]
    public void Setup()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IUserRepository>();
        _mockEmail = new Mock<IEmailService>();

        _mockUow.Setup(u => u.Users).Returns(_mockRepo.Object);

        _service = new UserService(_mockUow.Object, _mockEmail.Object);
    }

    [Test]
    public async Task GetAllAsync_ReturnsUsers()
    {
        var mockData = new List<User> { new() { Id = Guid.NewGuid(), Name = "John" } };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(mockData);

        var result = await _service.GetAllAsync();

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllPagedAsync_ReturnsPagedUsers()
    {
        var mockData = new List<User> { new() { Id = Guid.NewGuid(), Name = "John" } };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(mockData);

        var query = new PaginationQuery { PageNumber = 1, PageSize = 10 };
        var result = await _service.GetAllPagedAsync(query);

        Assert.That(result.Items.Count(), Is.EqualTo(1));
    }

    [Test]
    public void GetByIdAsync_ThrowsNotFound_WhenUserMissing()
    {
        Guid id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetByIdAsync(id));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsUser()
    {
        Guid id = Guid.NewGuid();
        var user = new User { Id = id, Name = "John" };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);

        var result = await _service.GetByIdAsync(id);

        Assert.That(result.Id, Is.EqualTo(id));
        Assert.That(result.Name, Is.EqualTo("John"));
    }

    [Test]
    public async Task GetByRoleAsync_ReturnsUsers()
    {
        var mockData = new List<User> { new() { Role = UserRole.SecurityStaff } };
        _mockRepo.Setup(r => r.GetByRoleAsync(UserRole.SecurityStaff)).ReturnsAsync(mockData);

        var result = await _service.GetByRoleAsync(UserRole.SecurityStaff);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void CreateAsync_ThrowsConflict_WhenEmailAlreadyExists()
    {
        var dto = new CreateUserDto { Email = "exists@test.com", Name = "John", Role = UserRole.Resident };
        _mockRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(new User());

        Assert.ThrowsAsync<ConflictException>(async () => await _service.CreateAsync(dto));
    }

    [Test]
    public async Task CreateAsync_CreatesAndSendsWelcomeEmail()
    {
        var dto = new CreateUserDto { Email = "new@test.com", Name = "John", PhoneNumber = "123", Role = UserRole.Resident };
        _mockRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User)null!);

        var result = await _service.CreateAsync(dto);

        Assert.That(result.Email, Is.EqualTo(dto.Email));
        Assert.That(result.IsActive, Is.True);
        _mockRepo.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == dto.Email && u.Name == dto.Name)), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockEmail.Verify(e => e.SendAsync(dto.Email, "Welcome to SmartSociety", It.IsAny<string>(), null, null), Times.Once);
    }

    [Test]
    public void UpdateAsync_ThrowsNotFound_WhenUserMissing()
    {
        Guid id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User)null!);

        var dto = new UpdateUserDto { Name = "Jane" };

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.UpdateAsync(id, dto));
    }

    [Test]
    public async Task UpdateAsync_UpdatesUserSuccessfully()
    {
        Guid id = Guid.NewGuid();
        var user = new User { Id = id, Name = "Old", PhoneNumber = "111" };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);

        var dto = new UpdateUserDto { Name = "New", PhoneNumber = "222" };

        var result = await _service.UpdateAsync(id, dto);

        Assert.That(result.Name, Is.EqualTo("New"));
        Assert.That(user.PhoneNumber, Is.EqualTo("222"));
        _mockRepo.Verify(r => r.UpdateAsync(user), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void ActivateAsync_ThrowsNotFound_WhenUserMissing()
    {
        Guid id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.ActivateAsync(id));
    }

    [Test]
    public async Task ActivateAsync_ActivatesUser()
    {
        Guid id = Guid.NewGuid();
        var user = new User { Id = id, IsActive = false };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);

        await _service.ActivateAsync(id);

        Assert.That(user.IsActive, Is.True);
        _mockRepo.Verify(r => r.UpdateAsync(user), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void DeactivateAsync_ThrowsNotFound_WhenUserMissing()
    {
        Guid id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.DeactivateAsync(id));
    }

    [Test]
    public async Task DeactivateAsync_DeactivatesUser()
    {
        Guid id = Guid.NewGuid();
        var user = new User { Id = id, IsActive = true };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);

        await _service.DeactivateAsync(id);

        Assert.That(user.IsActive, Is.False);
        _mockRepo.Verify(r => r.UpdateAsync(user), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void DeleteAsync_ThrowsNotFound_WhenUserMissing()
    {
        Guid id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.DeleteAsync(id));
    }

    [Test]
    public async Task DeleteAsync_DeletesUser()
    {
        Guid id = Guid.NewGuid();
        var user = new User { Id = id };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);

        await _service.DeleteAsync(id);

        _mockRepo.Verify(r => r.DeleteAsync(user), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
