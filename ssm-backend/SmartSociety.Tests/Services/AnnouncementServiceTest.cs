using Moq;
using NUnit.Framework;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Services;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class AnnouncementServiceTest
{
    private Mock<IUnitOfWork> _mockUow;
    private Mock<IAnnouncementRepository> _mockRepo;
    private AnnouncementService _service;

    [SetUp]
    public void SetUp()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IAnnouncementRepository>();

        _mockUow.Setup(uow => uow.Announcements).Returns(_mockRepo.Object);

        _service = new AnnouncementService(_mockUow.Object);
    }

    [Test]
    public async Task GetAllAsync_ReturnsCorrectValue()
    {
        var mockData = new List<Announcement>
        {
            new() { Id = Guid.NewGuid(), Title = "Title_1" }, 
            new() { Id = Guid.NewGuid(), Title = "Title_2" },
        };

        _mockRepo.Setup(a => a.GetAllAsync()).ReturnsAsync(mockData);

        var result = await _service.GetAllAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Title, Is.EqualTo("Title_1"));
    }

    [Test]
    public async Task GetAllPagedAsync_ReturnsCorrectValue()
    {
        var mockData = new List<Announcement>
        {
            new() { Id = Guid.NewGuid(), Title = "Search", Content = "Search demo" },
            new() { Id = Guid.NewGuid(), Title = "Title2", Content = "Content" },
        };

        _mockRepo.Setup(a => a.GetAllAsync()).ReturnsAsync(mockData);

        var query = new PaginationQuery
        {
            PageSize = 10,
            PageNumber = 1,
            Search = "Search",
        };

        var result = await _service.GetAllPagedAsync(query);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count(), Is.EqualTo(1));
    }

    [Test]
    public void GetByIdAsync_ThrowsNotFoundException()
    {
        Guid missingId = Guid.NewGuid();
        _mockRepo.Setup(a => a.GetByIdAsync(missingId)).ReturnsAsync((Announcement)null!);

        Assert.ThrowsAsync<NotFoundException>(
            async () => await _service.GetByIdAsync(missingId));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsAnnouncement()
    {
        Guid id = Guid.NewGuid();
        var announcement = new Announcement { Id = id, Title = "Found" };
        _mockRepo.Setup(a => a.GetByIdAsync(id)).ReturnsAsync(announcement);

        var result = await _service.GetByIdAsync(id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(id));
        Assert.That(result.Title, Is.EqualTo("Found"));
    }

    [Test]
    public async Task GetActiveByAudienceAsync_ReturnsCorrectValue()
    {
        var mockData = new List<Announcement> { new() { Id = Guid.NewGuid(), Audience = UserRole.Resident } };
        _mockRepo.Setup(a => a.GetActiveByAudienceAsync(UserRole.Resident)).ReturnsAsync(mockData);

        var result = await _service.GetActiveByAudienceAsync(UserRole.Resident);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetPinnedAsync_ReturnsCorrectValue()
    {
        var mockData = new List<Announcement> { new() { Id = Guid.NewGuid(), IsPinned = true } };
        _mockRepo.Setup(a => a.GetPinnedAsync()).ReturnsAsync(mockData);

        var result = await _service.GetPinnedAsync();

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void CreateAsync_ThrowsBadRequest_WhenExpiryInPast()
    {
        var dto = new AnnouncementDto
        {
            Title = "Past Expiry",
            Content = "Test",
            Audience = UserRole.Resident,
            IsPinned = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5)
        };

        Assert.ThrowsAsync<BadRequestException>(
            async () => await _service.CreateAsync(dto));
    }

    [Test]
    public async Task CreateAsync_CreatesAndReturnsAnnouncement()
    {
        var dto = new AnnouncementDto
        {
            Title = "Valid",
            Content = "Test",
            Audience = UserRole.Resident,
            IsPinned = false,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        var result = await _service.CreateAsync(dto);

        Assert.That(result.Title, Is.EqualTo(dto.Title));
        _mockRepo.Verify(a => a.AddAsync(It.IsAny<Announcement>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateAsync_ThrowsNotFound_WhenIdMissing()
    {
        Guid missingId = Guid.NewGuid();
        _mockRepo.Setup(a => a.GetByIdAsync(missingId)).ReturnsAsync((Announcement)null!);

        var dto = new AnnouncementDto { ExpiresAt = DateTime.UtcNow.AddDays(1) };

        Assert.ThrowsAsync<NotFoundException>(
            async () => await _service.UpdateAsync(missingId, dto));
    }

    [Test]
    public void UpdateAsync_ThrowsBadRequest_WhenExpiryInPast()
    {
        Guid id = Guid.NewGuid();
        var announcement = new Announcement { Id = id };
        _mockRepo.Setup(a => a.GetByIdAsync(id)).ReturnsAsync(announcement);

        var dto = new AnnouncementDto { ExpiresAt = DateTime.UtcNow.AddMinutes(-5) };

        Assert.ThrowsAsync<BadRequestException>(
            async () => await _service.UpdateAsync(id, dto));
    }

    [Test]
    public async Task UpdateAsync_UpdatesAndReturnsAnnouncement()
    {
        Guid id = Guid.NewGuid();
        var announcement = new Announcement { Id = id, Title = "Old" };
        _mockRepo.Setup(a => a.GetByIdAsync(id)).ReturnsAsync(announcement);

        var dto = new AnnouncementDto
        {
            Title = "New Title",
            Content = "New Content",
            Audience = UserRole.SecurityStaff,
            IsPinned = true,
            ExpiresAt = DateTime.UtcNow.AddDays(2)
        };

        var result = await _service.UpdateAsync(id, dto);

        Assert.That(result.Title, Is.EqualTo("New Title"));
        _mockRepo.Verify(a => a.UpdateAsync(announcement), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void DeleteAsync_ThrowsNotFound_WhenIdMissing()
    {
        Guid missingId = Guid.NewGuid();
        _mockRepo.Setup(a => a.GetByIdAsync(missingId)).ReturnsAsync((Announcement)null!);

        Assert.ThrowsAsync<NotFoundException>(
            async () => await _service.DeleteAsync(missingId));
    }

    [Test]
    public async Task DeleteAsync_DeletesAnnouncement()
    {
        Guid id = Guid.NewGuid();
        var announcement = new Announcement { Id = id };
        _mockRepo.Setup(a => a.GetByIdAsync(id)).ReturnsAsync(announcement);

        await _service.DeleteAsync(id);

        _mockRepo.Verify(a => a.DeleteAsync(announcement), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}