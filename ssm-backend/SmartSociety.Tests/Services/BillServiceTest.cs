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
public class BillServiceTest
{
    private Mock<IUnitOfWork> _mockUow;
    private Mock<IBillRepository> _mockBillRepo;
    private Mock<IApartmentRepository> _mockApartmentRepo;
    private Mock<IResidentRepository> _mockResidentRepo;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<INotificationService> _mockNotif;
    private BillService _service;

    [SetUp]
    public void Setup()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockBillRepo = new Mock<IBillRepository>();
        _mockApartmentRepo = new Mock<IApartmentRepository>();
        _mockResidentRepo = new Mock<IResidentRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockNotif = new Mock<INotificationService>();

        _mockUow.Setup(u => u.Bills).Returns(_mockBillRepo.Object);
        _mockUow.Setup(u => u.Apartments).Returns(_mockApartmentRepo.Object);
        _mockUow.Setup(u => u.Residents).Returns(_mockResidentRepo.Object);
        _mockUow.Setup(u => u.Users).Returns(_mockUserRepo.Object);

        _service = new BillService(_mockUow.Object, _mockNotif.Object);
    }

    [Test]
    public async Task GetAllAsync_ReturnsBills()
    {
        var mockData = new List<Bill> { new() { Id = Guid.NewGuid(), Period = "12-2026" } };
        _mockBillRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(mockData);

        var result = await _service.GetAllAsync();

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllPagedAsync_ReturnsPagedBills()
    {
        var mockData = new List<Bill> { new() { Id = Guid.NewGuid(), Period = "12-2026" } };
        _mockBillRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(mockData);

        var query = new PaginationQuery { PageNumber = 1, PageSize = 10 };
        var result = await _service.GetAllPagedAsync(query);

        Assert.That(result.Items.Count(), Is.EqualTo(1));
    }

    [Test]
    public void GetByIdAsync_ThrowsNotFound_WhenBillMissing()
    {
        Guid id = Guid.NewGuid();
        _mockBillRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Bill)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetByIdAsync(id));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsBill()
    {
        Guid id = Guid.NewGuid();
        Guid apartmentId = Guid.NewGuid();
        var bill = new Bill { Id = id, ApartmentId = apartmentId, Period = "12-2026" };
        _mockBillRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(bill);
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(apartmentId)).ReturnsAsync(new Apartment { Block = "A", Number = "101" });

        var result = await _service.GetByIdAsync(id);

        Assert.That(result.Id, Is.EqualTo(id));
        Assert.That(result.ApartmentBlock, Is.EqualTo("A"));
    }

    [Test]
    public async Task GetPendingBillsAsync_ReturnsPending()
    {
        var mockData = new List<Bill> { new() { Status = BillingStatus.Unpaid } };
        _mockBillRepo.Setup(r => r.GetPendingBillsAsync()).ReturnsAsync(mockData);

        var result = await _service.GetPendingBillsAsync();

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetByApartmentIdAsync_ReturnsBills()
    {
        Guid id = Guid.NewGuid();
        var mockData = new List<Bill> { new() { ApartmentId = id } };
        _mockBillRepo.Setup(r => r.GetByApartmentIdAsync(id)).ReturnsAsync(mockData);

        var result = await _service.GetByApartmentIdAsync(id);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetMyBillsAsync_ReturnsBillsForResident()
    {
        Guid userId = Guid.NewGuid();
        Guid apartmentId = Guid.NewGuid();
        var resident = new Resident { UserId = userId, ApartmentId = apartmentId };
        _mockResidentRepo.Setup(r => r.GetCurrentByUserIdAsync(userId)).ReturnsAsync(resident);

        var mockData = new List<Bill> { new() { ApartmentId = apartmentId } };
        _mockBillRepo.Setup(r => r.GetByApartmentIdAsync(apartmentId)).ReturnsAsync(mockData);

        var user = new User { Id = userId, Role = UserRole.Resident };
        _mockUserRepo.Setup(u => u.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _service.GetMyBillsAsync(userId);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void GetByUserIdAsync_ThrowsNotFound_WhenNoResidency()
    {
        Guid userId = Guid.NewGuid();
        _mockResidentRepo.Setup(r => r.GetCurrentByUserIdAsync(userId)).ReturnsAsync((Resident)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetByUserIdAsync(userId));
    }

    [Test]
    public async Task GetByUserIdAsync_ReturnsBills()
    {
        Guid userId = Guid.NewGuid();
        Guid apartmentId = Guid.NewGuid();
        var resident = new Resident { UserId = userId, ApartmentId = apartmentId };
        _mockResidentRepo.Setup(r => r.GetCurrentByUserIdAsync(userId)).ReturnsAsync(resident);

        var mockData = new List<Bill> { new() { ApartmentId = apartmentId } };
        _mockBillRepo.Setup(r => r.GetByApartmentIdAsync(apartmentId)).ReturnsAsync(mockData);

        var result = await _service.GetByUserIdAsync(userId);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetByPeriodAsync_ReturnsBills()
    {
        var mockData = new List<Bill> { new() { Period = "12-2026" } };
        _mockBillRepo.Setup(r => r.GetByPeriodAsync("12-2026")).ReturnsAsync(mockData);

        var result = await _service.GetByPeriodAsync("12-2026");

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void CreateAsync_ThrowsNotFound_WhenApartmentMissing()
    {
        var dto = new CreateBillDto { ApartmentId = Guid.NewGuid(), Period = "12-2026" };
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(dto.ApartmentId)).ReturnsAsync((Apartment)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.CreateAsync(dto));
    }

    [Test]
    public void CreateAsync_ThrowsConflict_WhenBillAlreadyExists()
    {
        var dto = new CreateBillDto { ApartmentId = Guid.NewGuid(), Period = "12-2026" };
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(dto.ApartmentId)).ReturnsAsync(new Apartment());
        _mockBillRepo.Setup(r => r.GetByApartmentAndPeriodAsync(dto.ApartmentId, dto.Period)).ReturnsAsync(new Bill());

        Assert.ThrowsAsync<ConflictException>(async () => await _service.CreateAsync(dto));
    }

    [Test]
    public void CreateAsync_ThrowsBadRequest_WhenDueDateInPast()
    {
        var dto = new CreateBillDto { ApartmentId = Guid.NewGuid(), Period = "12-2026", DueDate = DateTime.UtcNow.AddMinutes(-5) };
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(dto.ApartmentId)).ReturnsAsync(new Apartment());
        _mockBillRepo.Setup(r => r.GetByApartmentAndPeriodAsync(dto.ApartmentId, dto.Period)).ReturnsAsync((Bill)null!);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CreateAsync(dto));
    }

    [Test]
    public async Task CreateAsync_CreatesAndNotifiesBilledUser()
    {
        var dto = new CreateBillDto 
        { 
            ApartmentId = Guid.NewGuid(), 
            Period = "12-2026", 
            DueDate = DateTime.UtcNow.AddDays(5),
            BaseAmount = 1000
        };
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(dto.ApartmentId)).ReturnsAsync(new Apartment());
        _mockBillRepo.Setup(r => r.GetByApartmentAndPeriodAsync(dto.ApartmentId, dto.Period)).ReturnsAsync((Bill)null!);

        var resident = new Resident { UserId = Guid.NewGuid(), MoveOutDate = null };
        _mockResidentRepo.Setup(r => r.GetByApartmentIdAsync(dto.ApartmentId)).ReturnsAsync(new List<Resident> { resident });

        var result = await _service.CreateAsync(dto);

        Assert.That(result.Period, Is.EqualTo(dto.Period));
        Assert.That(result.BaseAmount, Is.EqualTo(dto.BaseAmount));
        _mockBillRepo.Verify(r => r.AddAsync(It.IsAny<Bill>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockNotif.Verify(n => n.CreateAsync(resident.UserId, "New Maintenance Bill", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ApplyPenaltiesAsync_AppliesPenaltyToOverdueBills()
    {
        Guid apartmentId = Guid.NewGuid();
        var overdueBill = new Bill
        {
            Id = Guid.NewGuid(),
            ApartmentId = apartmentId,
            Period = "12-2026",
            BaseAmount = 1000,
            DueDate = DateTime.UtcNow.AddDays(-1),
            Status = BillingStatus.Unpaid,
            BilledToUserId = Guid.NewGuid()
        };
        var activeBill = new Bill
        {
            Id = Guid.NewGuid(),
            BaseAmount = 1000,
            DueDate = DateTime.UtcNow.AddDays(5),
            Status = BillingStatus.Unpaid
        };

        _mockBillRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Bill> { overdueBill, activeBill });

        await _service.ApplyPenaltiesAsync();

        Assert.That(overdueBill.Status, Is.EqualTo(BillingStatus.Overdue));
        Assert.That(overdueBill.Penalty, Is.EqualTo(50.00m)); // 5% of 1000
        _mockBillRepo.Verify(r => r.UpdateAsync(overdueBill), Times.Once);
        _mockBillRepo.Verify(r => r.UpdateAsync(activeBill), Times.Never);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockNotif.Verify(n => n.CreateAsync(overdueBill.BilledToUserId, "Bill Overdue", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void DeleteAsync_ThrowsNotFound_WhenBillMissing()
    {
        Guid id = Guid.NewGuid();
        _mockBillRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Bill)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.DeleteAsync(id));
    }

    [Test]
    public void DeleteAsync_ThrowsBadRequest_WhenBillPaidOrDisputed()
    {
        Guid id = Guid.NewGuid();
        var bill = new Bill { Id = id, Status = BillingStatus.Paid };
        _mockBillRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(bill);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.DeleteAsync(id));

        bill.Status = BillingStatus.Disputed;
        Assert.ThrowsAsync<BadRequestException>(async () => await _service.DeleteAsync(id));
    }

    [Test]
    public async Task DeleteAsync_DeletesBillSuccessfully()
    {
        Guid id = Guid.NewGuid();
        var bill = new Bill { Id = id, Status = BillingStatus.Unpaid };
        _mockBillRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(bill);

        await _service.DeleteAsync(id);

        _mockBillRepo.Verify(r => r.DeleteAsync(bill), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
