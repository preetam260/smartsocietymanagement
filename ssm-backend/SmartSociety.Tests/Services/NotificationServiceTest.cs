using Moq;
using NUnit.Framework;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Services;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class NotificationServiceTest
{
    private Mock<IUnitOfWork> _mockUow;
    private Mock<INotificationRepository> _mockRepo;
    private Mock<IEmailService> _mockEmail;
    private NotificationService _service;

    [SetUp]
    public void Setup()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<INotificationRepository>();
        _mockEmail = new Mock<IEmailService>();

        _mockUow.Setup(u => u.Notifications).Returns(_mockRepo.Object);

        _service = new NotificationService(_mockUow.Object, _mockEmail.Object);
    }

    [Test]
    public async Task GetByUserIdAsync_ReturnsNotifications()
    {
        Guid userId = Guid.NewGuid();
        var mockData = new List<Notification> { new() { UserId = userId, Title = "Test" } };
        _mockRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(mockData);

        var result = await _service.GetByUserIdAsync(userId);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetByUserIdPagedAsync_ReturnsPagedNotifications()
    {
        Guid userId = Guid.NewGuid();
        var mockData = new List<Notification> { new() { UserId = userId, Title = "Test" } };
        _mockRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(mockData);

        var query = new PaginationQuery { PageNumber = 1, PageSize = 10 };
        var result = await _service.GetByUserIdPagedAsync(userId, query);

        Assert.That(result.Items.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetUnreadByUserIdAsync_ReturnsUnread()
    {
        Guid userId = Guid.NewGuid();
        var mockData = new List<Notification> { new() { UserId = userId, IsRead = false } };
        _mockRepo.Setup(r => r.GetUnreadByUserIdAsync(userId)).ReturnsAsync(mockData);

        var result = await _service.GetUnreadByUserIdAsync(userId);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void MarkAsReadAsync_ThrowsNotFound_WhenNotificationMissing()
    {
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Notification)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.MarkAsReadAsync(id, userId));
    }

    [Test]
    public async Task MarkAsReadAsync_MarksNotificationAsRead()
    {
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var notification = new Notification { Id = id, UserId = userId, IsRead = false };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(notification);

        await _service.MarkAsReadAsync(id, userId);

        Assert.That(notification.IsRead, Is.True);
        _mockRepo.Verify(r => r.UpdateAsync(notification), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task MarkAllAsReadAsync_MarksAllUnreadAsRead()
    {
        Guid userId = Guid.NewGuid();
        var notif1 = new Notification { UserId = userId, IsRead = false };
        var notif2 = new Notification { UserId = userId, IsRead = false };
        var unread = new List<Notification> { notif1, notif2 };

        _mockRepo.Setup(r => r.GetUnreadByUserIdAsync(userId)).ReturnsAsync(unread);

        await _service.MarkAllAsReadAsync(userId);

        Assert.That(notif1.IsRead, Is.True);
        Assert.That(notif2.IsRead, Is.True);
        _mockRepo.Verify(r => r.UpdateAsync(notif1), Times.Once);
        _mockRepo.Verify(r => r.UpdateAsync(notif2), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task CreateAsync_CreatesNotificationAndSendsEmail()
    {
        Guid userId = Guid.NewGuid();
        string title = "New Alert";
        string message = "Details";
        var user = new User { Id = userId, Email = "recipient@test.com", IsActive = true, IsDeleted = false };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(ur => ur.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockUow.Setup(u => u.Users).Returns(mockUserRepo.Object);

        await _service.CreateAsync(userId, title, message);

        _mockRepo.Verify(r => r.AddAsync(It.Is<Notification>(n => n.UserId == userId && n.Title == title && n.Message == message)), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockEmail.Verify(e => e.SendAsync(user.Email, $"SmartSociety — {title}", It.IsAny<string>(), null, null), Times.Once);
    }
}
