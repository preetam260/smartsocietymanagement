using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Http;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Services;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class VisitorServiceTest
{
    private Mock<IUnitOfWork> _mockUow;
    private Mock<IVisitorRepository> _mockVisitorRepo;
    private Mock<IVisitorEntryRepository> _mockEntryRepo;
    private Mock<IResidentRepository> _mockResidentRepo;
    private Mock<IApartmentRepository> _mockApartmentRepo;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<IQRService> _mockQr;
    private Mock<IEmailService> _mockEmail;
    private Mock<INotificationService> _mockNotif;
    private VisitorService _service;

    [SetUp]
    public void Setup()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockVisitorRepo = new Mock<IVisitorRepository>();
        _mockEntryRepo = new Mock<IVisitorEntryRepository>();
        _mockResidentRepo = new Mock<IResidentRepository>();
        _mockApartmentRepo = new Mock<IApartmentRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockQr = new Mock<IQRService>();
        _mockEmail = new Mock<IEmailService>();
        _mockNotif = new Mock<INotificationService>();

        _mockUow.Setup(u => u.Visitors).Returns(_mockVisitorRepo.Object);
        _mockUow.Setup(u => u.VisitorEntries).Returns(_mockEntryRepo.Object);
        _mockUow.Setup(u => u.Residents).Returns(_mockResidentRepo.Object);
        _mockUow.Setup(u => u.Apartments).Returns(_mockApartmentRepo.Object);
        _mockUow.Setup(u => u.Users).Returns(_mockUserRepo.Object);

        _service = new VisitorService(_mockUow.Object, _mockQr.Object, _mockEmail.Object, _mockNotif.Object);
    }

    [Test]
    public void GetByIdAsync_ThrowsNotFound_WhenVisitorMissing()
    {
        Guid id = Guid.NewGuid();
        _mockVisitorRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Visitor)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetByIdAsync(id));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsVisitor()
    {
        Guid id = Guid.NewGuid();
        Guid apartmentId = Guid.NewGuid();
        var visitor = new Visitor { Id = id, ApartmentId = apartmentId, Name = "Guest" };
        _mockVisitorRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(visitor);
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(apartmentId)).ReturnsAsync(new Apartment { Block = "B", Number = "202" });

        var result = await _service.GetByIdAsync(id);

        Assert.That(result.Id, Is.EqualTo(id));
        Assert.That(result.ApartmentBlock, Is.EqualTo("B"));
    }

    [Test]
    public async Task GetByApartmentIdAsync_ReturnsVisitors()
    {
        Guid id = Guid.NewGuid();
        var mockData = new List<Visitor> { new() { ApartmentId = id } };
        _mockVisitorRepo.Setup(r => r.GetByApartmentIdAsync(id)).ReturnsAsync(mockData);

        var result = await _service.GetByApartmentIdAsync(id);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetByApartmentIdPagedAsync_ReturnsPagedVisitors()
    {
        Guid id = Guid.NewGuid();
        var mockData = new List<Visitor> { new() { ApartmentId = id } };
        _mockVisitorRepo.Setup(r => r.GetByApartmentIdAsync(id)).ReturnsAsync(mockData);

        var query = new PaginationQuery { PageNumber = 1, PageSize = 10 };
        var result = await _service.GetByApartmentIdPagedAsync(id, query);

        Assert.That(result.Items.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetByStatusAsync_ReturnsVisitors()
    {
        var mockData = new List<Visitor> { new() { Status = VisitorStatus.Approved } };
        _mockVisitorRepo.Setup(r => r.GetByStatusAsync(VisitorStatus.Approved)).ReturnsAsync(mockData);

        var result = await _service.GetByStatusAsync(VisitorStatus.Approved);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void GetMyVisitorsAsync_ThrowsNotFound_WhenNoResidency()
    {
        Guid userId = Guid.NewGuid();
        _mockResidentRepo.Setup(r => r.GetCurrentByUserIdAsync(userId)).ReturnsAsync((Resident)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetMyVisitorsAsync(userId));
    }

    [Test]
    public async Task GetMyVisitorsAsync_ReturnsVisitors()
    {
        Guid userId = Guid.NewGuid();
        Guid apartmentId = Guid.NewGuid();
        var resident = new Resident { UserId = userId, ApartmentId = apartmentId };
        _mockResidentRepo.Setup(r => r.GetCurrentByUserIdAsync(userId)).ReturnsAsync(resident);

        var mockData = new List<Visitor> { new() { ApartmentId = apartmentId } };
        _mockVisitorRepo.Setup(r => r.GetByApartmentIdAsync(apartmentId)).ReturnsAsync(mockData);

        var result = await _service.GetMyVisitorsAsync(userId);

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
    public async Task GetByUserIdAsync_ReturnsVisitors()
    {
        Guid userId = Guid.NewGuid();
        Guid apartmentId = Guid.NewGuid();
        var resident = new Resident { UserId = userId, ApartmentId = apartmentId };
        _mockResidentRepo.Setup(r => r.GetCurrentByUserIdAsync(userId)).ReturnsAsync(resident);

        var mockData = new List<Visitor> { new() { ApartmentId = apartmentId } };
        _mockVisitorRepo.Setup(r => r.GetByApartmentIdAsync(apartmentId)).ReturnsAsync(mockData);

        var result = await _service.GetByUserIdAsync(userId);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void RegisterAsync_ThrowsNotFound_WhenNoResidency()
    {
        Guid userId = Guid.NewGuid();
        _mockResidentRepo.Setup(r => r.GetCurrentByUserIdAsync(userId)).ReturnsAsync((Resident)null!);

        var dto = new RegisterVisitorDto { ETA = DateTime.UtcNow.AddHours(2) };

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.RegisterAsync(dto, userId));
    }

    [Test]
    public void RegisterAsync_ThrowsBadRequest_WhenETAInPast()
    {
        Guid userId = Guid.NewGuid();
        var resident = new Resident { UserId = userId };
        _mockResidentRepo.Setup(r => r.GetCurrentByUserIdAsync(userId)).ReturnsAsync(resident);

        var dto = new RegisterVisitorDto { ETA = DateTime.UtcNow.AddMinutes(-5) };

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.RegisterAsync(dto, userId));
    }

    [Test]
    public async Task RegisterAsync_RegistersAndSendsEmail()
    {
        Guid userId = Guid.NewGuid();
        Guid apartmentId = Guid.NewGuid();
        var resident = new Resident { UserId = userId, ApartmentId = apartmentId };
        _mockResidentRepo.Setup(r => r.GetCurrentByUserIdAsync(userId)).ReturnsAsync(resident);

        var dto = new RegisterVisitorDto { Name = "Guest", Email = "guest@test.com", Purpose = "Visit", ETA = DateTime.UtcNow.AddHours(2) };
        _mockQr.Setup(q => q.GenerateTokenAsync()).ReturnsAsync("qr_tok");
        _mockQr.Setup(q => q.GenerateImageAsync("qr_tok")).ReturnsAsync(new byte[] { 1, 2, 3 });
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(apartmentId)).ReturnsAsync(new Apartment { Block = "B", Number = "202" });

        var result = await _service.RegisterAsync(dto, userId);

        Assert.That(result.Name, Is.EqualTo(dto.Name));
        Assert.That(result.QrToken, Is.EqualTo("qr_tok"));
        _mockVisitorRepo.Verify(r => r.AddAsync(It.Is<Visitor>(v => v.Name == dto.Name && v.QrToken == "qr_tok")), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockEmail.Verify(e => e.SendAsync(dto.Email, "Your SmartSociety Visitor Pass", It.IsAny<string>(), It.IsAny<byte[]>(), "qrcode.png"), Times.Once);
    }

    [Test]
    public void DenyAsync_ThrowsNotFound_WhenVisitorMissing()
    {
        Guid id = Guid.NewGuid();
        _mockVisitorRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Visitor)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.DenyAsync(id));
    }

    [Test]
    public void DenyAsync_ThrowsBadRequest_WhenNotApproved()
    {
        Guid id = Guid.NewGuid();
        var visitor = new Visitor { Id = id, Status = VisitorStatus.CheckedIn };
        _mockVisitorRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(visitor);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.DenyAsync(id));
    }

    [Test]
    public async Task DenyAsync_DeniesVisitorAndNotifiesResidents()
    {
        Guid id = Guid.NewGuid();
        Guid apartmentId = Guid.NewGuid();
        var visitor = new Visitor { Id = id, ApartmentId = apartmentId, Status = VisitorStatus.Approved, Name = "Guest" };
        _mockVisitorRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(visitor);

        var resident = new Resident { UserId = Guid.NewGuid(), MoveOutDate = null };
        _mockResidentRepo.Setup(r => r.GetByApartmentIdAsync(apartmentId)).ReturnsAsync(new List<Resident> { resident });

        await _service.DenyAsync(id);

        Assert.That(visitor.Status, Is.EqualTo(VisitorStatus.Denied));
        _mockVisitorRepo.Verify(r => r.UpdateAsync(visitor), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockNotif.Verify(n => n.CreateAsync(resident.UserId, "Visitor Entry Denied", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void CheckInAsync_ThrowsNotFound_WhenTokenMissing()
    {
        _mockVisitorRepo.Setup(r => r.GetByQrTokenAsync("tok")).ReturnsAsync((Visitor)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.CheckInAsync("tok", Guid.NewGuid()));
    }

    [Test]
    public void CheckInAsync_ThrowsBadRequest_WhenNotApproved()
    {
        var visitor = new Visitor { Status = VisitorStatus.CheckedIn };
        _mockVisitorRepo.Setup(r => r.GetByQrTokenAsync("tok")).ReturnsAsync(visitor);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CheckInAsync("tok", Guid.NewGuid()));
    }

    [Test]
    public void CheckInAsync_ThrowsBadRequest_WhenPassExpired()
    {
        var visitor = new Visitor { Status = VisitorStatus.Approved, ExpiresAt = DateTime.UtcNow.AddMinutes(-5) };
        _mockVisitorRepo.Setup(r => r.GetByQrTokenAsync("tok")).ReturnsAsync(visitor);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CheckInAsync("tok", Guid.NewGuid()));
        Assert.That(visitor.Status, Is.EqualTo(VisitorStatus.Expired));
        _mockVisitorRepo.Verify(r => r.UpdateAsync(visitor), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CheckInAsync_ThrowsBadRequest_WhenAlreadyCheckedIn()
    {
        var visitor = new Visitor { Id = Guid.NewGuid(), Status = VisitorStatus.Approved, ExpiresAt = DateTime.UtcNow.AddHours(1) };
        _mockVisitorRepo.Setup(r => r.GetByQrTokenAsync("tok")).ReturnsAsync(visitor);

        var activeEntry = new VisitorEntry();
        _mockEntryRepo.Setup(e => e.GetActiveEntryAsync(visitor.Id)).ReturnsAsync(activeEntry);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CheckInAsync("tok", Guid.NewGuid()));
    }

    [Test]
    public async Task CheckInAsync_ChecksInVisitorAndNotifiesResidents()
    {
        Guid staffId = Guid.NewGuid();
        Guid apartmentId = Guid.NewGuid();
        var visitor = new Visitor { Id = Guid.NewGuid(), ApartmentId = apartmentId, Status = VisitorStatus.Approved, ExpiresAt = DateTime.UtcNow.AddHours(1), Name = "Guest" };
        _mockVisitorRepo.Setup(r => r.GetByQrTokenAsync("tok")).ReturnsAsync(visitor);

        _mockEntryRepo.Setup(e => e.GetActiveEntryAsync(visitor.Id)).ReturnsAsync((VisitorEntry)null!);
        _mockUserRepo.Setup(u => u.GetByIdAsync(staffId)).ReturnsAsync(new User { Name = "Guard" });

        var resident = new Resident { UserId = Guid.NewGuid(), MoveOutDate = null };
        _mockResidentRepo.Setup(r => r.GetByApartmentIdAsync(apartmentId)).ReturnsAsync(new List<Resident> { resident });

        var result = await _service.CheckInAsync("tok", staffId);

        Assert.That(visitor.Status, Is.EqualTo(VisitorStatus.CheckedIn));
        Assert.That(result.VisitorName, Is.EqualTo("Guest"));
        Assert.That(result.StaffName, Is.EqualTo("Guard"));

        _mockEntryRepo.Verify(e => e.AddAsync(It.IsAny<VisitorEntry>()), Times.Once);
        _mockVisitorRepo.Verify(r => r.UpdateAsync(visitor), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockNotif.Verify(n => n.CreateAsync(resident.UserId, "Visitor Arrived", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void CheckOutAsync_ThrowsNotFound_WhenVisitorMissing()
    {
        Guid id = Guid.NewGuid();
        _mockVisitorRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Visitor)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.CheckOutAsync(id, Guid.NewGuid()));
    }

    [Test]
    public void CheckOutAsync_ThrowsBadRequest_WhenNotCheckedIn()
    {
        Guid id = Guid.NewGuid();
        var visitor = new Visitor { Id = id, Status = VisitorStatus.Approved };
        _mockVisitorRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(visitor);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CheckOutAsync(id, Guid.NewGuid()));
    }

    [Test]
    public void CheckOutAsync_ThrowsNotFound_WhenNoActiveEntry()
    {
        Guid id = Guid.NewGuid();
        var visitor = new Visitor { Id = id, Status = VisitorStatus.CheckedIn };
        _mockVisitorRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(visitor);
        _mockEntryRepo.Setup(e => e.GetActiveEntryAsync(id)).ReturnsAsync((VisitorEntry)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.CheckOutAsync(id, Guid.NewGuid()));
    }

    [Test]
    public async Task CheckOutAsync_ChecksOutVisitorAndNotifiesResidents()
    {
        Guid id = Guid.NewGuid();
        Guid staffId = Guid.NewGuid();
        Guid apartmentId = Guid.NewGuid();
        var visitor = new Visitor { Id = id, ApartmentId = apartmentId, Status = VisitorStatus.CheckedIn, Name = "Guest" };
        _mockVisitorRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(visitor);

        var activeEntry = new VisitorEntry { Id = Guid.NewGuid(), VisitorId = id };
        _mockEntryRepo.Setup(e => e.GetActiveEntryAsync(id)).ReturnsAsync(activeEntry);
        _mockUserRepo.Setup(u => u.GetByIdAsync(staffId)).ReturnsAsync(new User { Name = "Guard" });

        var resident = new Resident { UserId = Guid.NewGuid(), MoveOutDate = null };
        _mockResidentRepo.Setup(r => r.GetByApartmentIdAsync(apartmentId)).ReturnsAsync(new List<Resident> { resident });

        var result = await _service.CheckOutAsync(id, staffId);

        Assert.That(visitor.Status, Is.EqualTo(VisitorStatus.CheckedOut));
        Assert.That(activeEntry.CheckoutTime, Is.Not.Null);

        _mockEntryRepo.Verify(e => e.UpdateAsync(activeEntry), Times.Once);
        _mockVisitorRepo.Verify(r => r.UpdateAsync(visitor), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockNotif.Verify(n => n.CreateAsync(resident.UserId, "Visitor Departed", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void GetEntriesByVisitorIdAsync_ThrowsNotFound_WhenVisitorMissing()
    {
        Guid id = Guid.NewGuid();
        _mockVisitorRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Visitor)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetEntriesByVisitorIdAsync(id));
    }

    [Test]
    public async Task GetEntriesByVisitorIdAsync_ReturnsEntries()
    {
        Guid visitorId = Guid.NewGuid();
        Guid staffId = Guid.NewGuid();
        var visitor = new Visitor { Id = visitorId, Name = "Guest" };
        _mockVisitorRepo.Setup(r => r.GetByIdAsync(visitorId)).ReturnsAsync(visitor);

        var entries = new List<VisitorEntry> { new() { VisitorId = visitorId, StaffId = staffId } };
        _mockEntryRepo.Setup(e => e.GetByVisitorIdAsync(visitorId)).ReturnsAsync(entries);
        _mockUserRepo.Setup(u => u.GetByIdAsync(staffId)).ReturnsAsync(new User { Name = "Guard" });

        var result = await _service.GetEntriesByVisitorIdAsync(visitorId);

        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().VisitorName, Is.EqualTo("Guest"));
    }
}
