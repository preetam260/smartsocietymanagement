using Moq;
using NUnit.Framework;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Services;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class ApartmentServiceTest
{
    private Mock<IUnitOfWork> _mockUow;
    private Mock<IApartmentRepository> _mockRepo;
    private Mock<IUserRepository> _mockUserRepo;
    private ApartmentService _service;

    [SetUp]
    public void Setup()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IApartmentRepository>();
        _mockUserRepo = new Mock<IUserRepository>();

        _mockUow.Setup(uow => uow.Apartments).Returns(_mockRepo.Object);
        _mockUow.Setup(uow => uow.Users).Returns(_mockUserRepo.Object);

        // Default mock setup for Users
        _mockUserRepo.Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new User 
            { 
                Id = id, 
                Name = "Test Owner", 
                Role = SmartSociety.Domain.Enums.UserRole.Owner 
            });

        _service = new ApartmentService(_mockUow.Object);
    }

    [Test]
    public async Task GetAllAsync_ReturnsCorrectValues()
    {
        var mockData = new List<Apartment>
        {
            new() { Id = Guid.NewGuid(), Number = "101" },
            new() { Id = Guid.NewGuid(), Number = "102" }
        };

        _mockRepo.Setup(a => a.GetAllAsync()).ReturnsAsync(mockData);

        var result = await _service.GetAllAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllPagedAsync_ReturnsCorrectValues_WithCorrectQuery()
    {
        var mockData = new List<Apartment>
        {
            new() { Id = Guid.NewGuid(), Number = "101", Floor = 1 },
            new() { Id = Guid.NewGuid(), Number = "102", Floor = 1 },
        };

        _mockRepo.Setup(a => a.GetAllAsync()).ReturnsAsync(mockData);

        PaginationQuery query = new PaginationQuery
        {
            PageNumber = 1,
            PageSize = 10
        };
        var result = await _service.GetAllPagedAsync(query);

        Assert.That(result.Items.Count(), Is.EqualTo(2));
    }

    [Test]
    public void GetByIdAsync_ApartmentNotFound_ThrowsException()
    {
        Guid id = Guid.NewGuid();
        _mockRepo.Setup(a => a.GetByIdAsync(id)).ReturnsAsync((Apartment)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetByIdAsync(id));
    }

    [Test]
    public async Task GetByIdAsync_ApartmentFound_ReturnsApartment()
    {
        Guid id = Guid.NewGuid();
        var apartment = new Apartment { Id = id, Block = "A", Number = "101", Floor = 1 };
        _mockRepo.Setup(a => a.GetByIdAsync(id)).ReturnsAsync(apartment);

        var result = await _service.GetByIdAsync(id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(id));
        Assert.That(result.Number, Is.EqualTo("101"));
    }

    [Test]
    public void CreateAsync_ThrowsConflictException_WhenAlreadyExists()
    {
        var dto = new CreateApartmentDto { Block = "A", Number = "101", Floor = 1 };
        var existing = new Apartment { Id = Guid.NewGuid(), Block = "A", Number = "101" };
        _mockRepo.Setup(a => a.GetByBlockAndNumberAsync("A", "101")).ReturnsAsync(existing);

        Assert.ThrowsAsync<ConflictException>(async () => await _service.CreateAsync(dto));
    }

    [Test]
    public async Task CreateAsync_CreatesAndReturnsApartment()
    {
        var dto = new CreateApartmentDto { Block = "A", Number = "101", Floor = 1 };
        _mockRepo.Setup(a => a.GetByBlockAndNumberAsync("A", "101")).ReturnsAsync((Apartment)null!);

        var result = await _service.CreateAsync(dto);

        Assert.That(result.Block, Is.EqualTo("A"));
        Assert.That(result.Number, Is.EqualTo("101"));
        _mockRepo.Verify(a => a.AddAsync(It.IsAny<Apartment>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void DeleteAsync_ThrowsNotFoundException_WhenMissing()
    {
        Guid id = Guid.NewGuid();
        _mockRepo.Setup(a => a.GetByIdAsync(id)).ReturnsAsync((Apartment)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.DeleteAsync(id));
    }

    [Test]
    public async Task DeleteAsync_DeletesApartment()
    {
        Guid id = Guid.NewGuid();
        var apartment = new Apartment { Id = id };
        _mockRepo.Setup(a => a.GetByIdAsync(id)).ReturnsAsync(apartment);

        await _service.DeleteAsync(id);

        _mockRepo.Verify(a => a.DeleteAsync(apartment), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}