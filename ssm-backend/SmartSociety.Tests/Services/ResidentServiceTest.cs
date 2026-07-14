using Moq;
using NUnit.Framework;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Application.Services;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class ResidentServiceTest
{
    private Mock<IUnitOfWork> _mockUow;
    private Mock<IResidentRepository> _mockResidentRepo;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<IApartmentRepository> _mockApartmentRepo;
    private Mock<INotificationService> _mockNotificationService;
    private Mock<IEmailService> _mockEmailService;
    private ResidentService _service;

    [SetUp]
    public void Setup()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockResidentRepo = new Mock<IResidentRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockApartmentRepo = new Mock<IApartmentRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockEmailService = new Mock<IEmailService>();

        _mockUow.Setup(uow => uow.Residents).Returns(_mockResidentRepo.Object);
        _mockUow.Setup(uow => uow.Users).Returns(_mockUserRepo.Object);
        _mockUow.Setup(uow => uow.Apartments).Returns(_mockApartmentRepo.Object);

        _service = new ResidentService(_mockUow.Object, _mockNotificationService.Object, _mockEmailService.Object);
    }

    [Test]
    public async Task GetAllAsync_ReturnsCorrectValues()
    {
        var mockData = new List<Resident> { new() { Id = Guid.NewGuid() } };
        _mockResidentRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(mockData);

        var result = await _service.GetAllAsync();

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllCurrentAsync_ReturnsCorrectValues()
    {
        var mockData = new List<Resident> { new() { Id = Guid.NewGuid(), MoveOutDate = null } };
        _mockResidentRepo.Setup(r => r.GetAllCurrentAsync()).ReturnsAsync(mockData);

        var result = await _service.GetAllCurrentAsync();

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void GetByIdAsync_ThrowsNotFound_WhenMissing()
    {
        Guid id = Guid.NewGuid();
        _mockResidentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Resident)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetByIdAsync(id));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsResident()
    {
        Guid id = Guid.NewGuid();
        var resident = new Resident { Id = id };
        _mockResidentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(resident);

        var result = await _service.GetByIdAsync(id);

        Assert.That(result.Id, Is.EqualTo(id));
    }

    [Test]
    public async Task GetByApartmentIdAsync_ReturnsResidents()
    {
        Guid id = Guid.NewGuid();
        var mockData = new List<Resident> { new() { ApartmentId = id } };
        _mockResidentRepo.Setup(r => r.GetByApartmentIdAsync(id)).ReturnsAsync(mockData);

        var result = await _service.GetByApartmentIdAsync(id);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void GetCurrentByUserIdAsync_ThrowsNotFound_WhenMissing()
    {
        Guid id = Guid.NewGuid();
        _mockResidentRepo.Setup(r => r.GetCurrentByUserIdAsync(id)).ReturnsAsync((Resident)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetCurrentByUserIdAsync(id));
    }

    [Test]
    public async Task GetCurrentByUserIdAsync_ReturnsResident()
    {
        Guid id = Guid.NewGuid();
        var resident = new Resident { UserId = id };
        _mockResidentRepo.Setup(r => r.GetCurrentByUserIdAsync(id)).ReturnsAsync(resident);

        var result = await _service.GetCurrentByUserIdAsync(id);

        Assert.That(result.UserId, Is.EqualTo(id));
    }

    [Test]
    public void CreateAsync_ThrowsNotFound_WhenUserMissing()
    {
        var dto = new CreateResidentDto { UserId = Guid.NewGuid(), ApartmentId = Guid.NewGuid() };
        _mockUserRepo.Setup(u => u.GetByIdAsync(dto.UserId)).ReturnsAsync((User)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.CreateAsync(dto));
    }

    [Test]
    public void CreateAsync_ThrowsNotFound_WhenApartmentMissing()
    {
        var dto = new CreateResidentDto { UserId = Guid.NewGuid(), ApartmentId = Guid.NewGuid() };
        _mockUserRepo.Setup(u => u.GetByIdAsync(dto.UserId)).ReturnsAsync(new User { Role = SmartSociety.Domain.Enums.UserRole.Resident });
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(dto.ApartmentId)).ReturnsAsync((Apartment)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.CreateAsync(dto));
    }

    [Test]
    public void CreateAsync_Tenant_ThrowsConflict_WhenUserAlreadyHasActiveResidency()
    {
        var dto = new CreateResidentDto { UserId = Guid.NewGuid(), ApartmentId = Guid.NewGuid() };
        _mockUserRepo.Setup(u => u.GetByIdAsync(dto.UserId)).ReturnsAsync(new User { Role = SmartSociety.Domain.Enums.UserRole.Resident });
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(dto.ApartmentId)).ReturnsAsync(new Apartment());

        var existing = new Resident { Id = Guid.NewGuid() };
        _mockResidentRepo.Setup(r => r.GetCurrentByUserIdAsync(dto.UserId)).ReturnsAsync(existing);

        Assert.ThrowsAsync<ConflictException>(async () => await _service.CreateAsync(dto));
    }

    [Test]
    public async Task CreateAsync_CreatesAndReturnsResident()
    {
        var dto = new CreateResidentDto { UserId = Guid.NewGuid(), ApartmentId = Guid.NewGuid() };
        _mockUserRepo.Setup(u => u.GetByIdAsync(dto.UserId)).ReturnsAsync(new User { Role = SmartSociety.Domain.Enums.UserRole.Resident });
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(dto.ApartmentId)).ReturnsAsync(new Apartment());

        _mockResidentRepo.Setup(r => r.GetCurrentByUserIdAsync(dto.UserId)).ReturnsAsync((Resident)null!);

        var result = await _service.CreateAsync(dto);

        Assert.That(result.UserId, Is.EqualTo(dto.UserId));
        _mockResidentRepo.Verify(r => r.AddAsync(It.IsAny<Resident>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateAsync_ThrowsNotFound_WhenMissing()
    {
        Guid id = Guid.NewGuid();
        _mockResidentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Resident)null!);

        var dto = new UpdateResidentDto { VehicleNumber = "xyz" };

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.UpdateAsync(id, dto));
    }

    [Test]
    public async Task UpdateAsync_UpdatesVehicleNumber()
    {
        Guid id = Guid.NewGuid();
        var resident = new Resident { Id = id, VehicleNumber = "abc" };
        _mockResidentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(resident);

        var dto = new UpdateResidentDto { VehicleNumber = "xyz" };

        var result = await _service.UpdateAsync(id, dto);

        Assert.That(result.VehicleNumber, Is.EqualTo("xyz"));
        _mockResidentRepo.Verify(r => r.UpdateAsync(resident), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void MoveOutAsync_ThrowsNotFound_WhenMissing()
    {
        Guid id = Guid.NewGuid();
        _mockResidentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Resident)null!);

        var dto = new MoveOutResidentDto { MoveOutDate = DateTime.UtcNow };

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.MoveOutAsync(id, dto));
    }

    [Test]
    public void MoveOutAsync_ThrowsBadRequest_WhenAlreadyMovedOut()
    {
        Guid id = Guid.NewGuid();
        var resident = new Resident { Id = id, MoveOutDate = DateTime.UtcNow.AddDays(-1) };
        _mockResidentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(resident);

        var dto = new MoveOutResidentDto { MoveOutDate = DateTime.UtcNow };

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.MoveOutAsync(id, dto));
    }

    [Test]
    public void MoveOutAsync_ThrowsBadRequest_WhenMoveOutBeforeMoveIn()
    {
        Guid id = Guid.NewGuid();
        var resident = new Resident { Id = id, MoveInDate = DateTime.UtcNow };
        _mockResidentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(resident);

        var dto = new MoveOutResidentDto { MoveOutDate = DateTime.UtcNow.AddDays(-1) };

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.MoveOutAsync(id, dto));
    }

    [Test]
    public async Task MoveOutAsync_SetsMoveOutDateAndTransfersBills()
    {
        Guid id = Guid.NewGuid();
        Guid apartmentId = Guid.NewGuid();
        Guid ownerId = Guid.NewGuid();
        var resident = new Resident { Id = id, ApartmentId = apartmentId, UserId = Guid.NewGuid(), MoveInDate = DateTime.UtcNow.AddDays(-5) };
        _mockResidentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(resident);
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(apartmentId)).ReturnsAsync(new Apartment { Id = apartmentId, OwnerId = ownerId });
        _mockUserRepo.Setup(u => u.GetByIdAsync(resident.UserId)).ReturnsAsync(new User { Id = resident.UserId, Role = Domain.Enums.UserRole.Resident, Name = "Test Resident", Email = "test@res.com" });

        var mockBillRepo = new Mock<IBillRepository>();
        var unpaidBill = new Bill { Id = Guid.NewGuid(), ApartmentId = apartmentId, BilledToUserId = resident.UserId, Status = Domain.Enums.BillingStatus.Unpaid };
        mockBillRepo.Setup(b => b.GetByApartmentIdAsync(apartmentId)).ReturnsAsync(new List<Bill> { unpaidBill });
        _mockUow.Setup(u => u.Bills).Returns(mockBillRepo.Object);

        var moveOutDate = DateTime.UtcNow;
        var dto = new MoveOutResidentDto { MoveOutDate = moveOutDate };

        await _service.MoveOutAsync(id, dto);

        Assert.That(resident.MoveOutDate, Is.EqualTo(moveOutDate));
        Assert.That(unpaidBill.BilledToUserId, Is.EqualTo(ownerId)); // Bill transferred to owner
        _mockResidentRepo.Verify(r => r.UpdateAsync(resident), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}