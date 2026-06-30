using Moq;
using NUnit.Framework;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Services;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class FacilityServiceTest
{
    private Mock<IUnitOfWork> _mockUow;
    private Mock<IFacilityRepository> _mockRepo;
    private FacilityService _service;

    [SetUp]
    public void Setup()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IFacilityRepository>();

        _mockUow.Setup(u => u.Facilities).Returns(_mockRepo.Object);

        _service = new FacilityService(_mockUow.Object);
    }

    [Test]
    public async Task GetAllAsync_ReturnsFacilities()
    {
        var mockData = new List<Facility> { new() { Id = Guid.NewGuid(), Name = "Gym" } };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(mockData);

        var result = await _service.GetAllAsync();

        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("Gym"));
    }

    [Test]
    public async Task GetActiveAsync_ReturnsActiveFacilities()
    {
        var mockData = new List<Facility> { new() { Id = Guid.NewGuid(), Name = "Gym", IsActive = true } };
        _mockRepo.Setup(r => r.GetActiveFacilitiesAsync()).ReturnsAsync(mockData);

        var result = await _service.GetActiveAsync();

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void GetByIdAsync_ThrowsNotFound_WhenFacilityMissing()
    {
        Guid id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Facility)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetByIdAsync(id));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsFacility()
    {
        Guid id = Guid.NewGuid();
        var facility = new Facility { Id = id, Name = "Gym" };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(facility);

        var result = await _service.GetByIdAsync(id);

        Assert.That(result.Id, Is.EqualTo(id));
        Assert.That(result.Name, Is.EqualTo("Gym"));
    }

    [Test]
    public async Task CreateAsync_CreatesAndReturnsFacility()
    {
        var dto = new FacilityDto { Name = "Gym", Description = "Weight area", HourlyRate = 50, Capacity = 20, IsActive = true };

        var result = await _service.CreateAsync(dto);

        Assert.That(result.Name, Is.EqualTo(dto.Name));
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Facility>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateAsync_ThrowsNotFound_WhenFacilityMissing()
    {
        Guid id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Facility)null!);

        var dto = new FacilityDto { Name = "New Name" };

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.UpdateAsync(id, dto));
    }

    [Test]
    public async Task UpdateAsync_UpdatesAndReturnsFacility()
    {
        Guid id = Guid.NewGuid();
        var facility = new Facility { Id = id, Name = "Old Name" };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(facility);

        var dto = new FacilityDto { Name = "New Name", Description = "Desc", HourlyRate = 10, Capacity = 5, IsActive = false };

        var result = await _service.UpdateAsync(id, dto);

        Assert.That(result.Name, Is.EqualTo("New Name"));
        _mockRepo.Verify(r => r.UpdateAsync(facility), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void DeleteAsync_ThrowsNotFound_WhenFacilityMissing()
    {
        Guid id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Facility)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.DeleteAsync(id));
    }

    [Test]
    public async Task DeleteAsync_DeletesFacility()
    {
        Guid id = Guid.NewGuid();
        var facility = new Facility { Id = id };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(facility);

        await _service.DeleteAsync(id);

        _mockRepo.Verify(r => r.DeleteAsync(facility), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
