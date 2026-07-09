using Moq;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore.Storage;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Services;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class BookingServiceTest
{
    private Mock<IUnitOfWork> _mockUow;
    private Mock<IBookingRepository> _mockBookingRepo;
    private Mock<IFacilityRepository> _mockFacilityRepo;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<INotificationService> _mockNotif;
    private Mock<IDbContextTransaction> _mockTransaction;
    private BookingService _service;

    [SetUp]
    public void Setup()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockFacilityRepo = new Mock<IFacilityRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockNotif = new Mock<INotificationService>();
        _mockTransaction = new Mock<IDbContextTransaction>();

        _mockUow.Setup(u => u.Bookings).Returns(_mockBookingRepo.Object);
        _mockUow.Setup(u => u.Facilities).Returns(_mockFacilityRepo.Object);
        _mockUow.Setup(u => u.Users).Returns(_mockUserRepo.Object);
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(_mockTransaction.Object);

        _service = new BookingService(_mockUow.Object, _mockNotif.Object);
    }

    [Test]
    public void GetByIdAsync_ThrowsNotFound_WhenBookingMissing()
    {
        Guid id = Guid.NewGuid();
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync((Booking)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetByIdAsync(id));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsBooking()
    {
        Guid id = Guid.NewGuid();
        Guid facilityId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var booking = new Booking { Id = id, FacilityId = facilityId, UserId = userId };
        
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync(booking);
        _mockFacilityRepo.Setup(f => f.GetByIdAsync(facilityId)).ReturnsAsync(new Facility { Name = "Gym" });
        _mockUserRepo.Setup(u => u.GetByIdAsync(userId)).ReturnsAsync(new User { Name = "John" });

        var result = await _service.GetByIdAsync(id);

        Assert.That(result.Id, Is.EqualTo(id));
        Assert.That(result.FacilityName, Is.EqualTo("Gym"));
        Assert.That(result.UserName, Is.EqualTo("John"));
    }

    [Test]
    public async Task GetByFacilityIdAsync_ReturnsBookings()
    {
        Guid facilityId = Guid.NewGuid();
        var bookings = new List<Booking> { new() { FacilityId = facilityId } };
        _mockBookingRepo.Setup(b => b.GetAllBookingsByFacilityAsync(facilityId)).ReturnsAsync(bookings);

        var result = await _service.GetByFacilityIdAsync(facilityId);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetByFacilityIdPagedAsync_ReturnsPagedBookings()
    {
        Guid facilityId = Guid.NewGuid();
        var bookings = new List<Booking> { new() { FacilityId = facilityId } };
        _mockBookingRepo.Setup(b => b.GetAllBookingsByFacilityAsync(facilityId)).ReturnsAsync(bookings);

        var query = new PaginationQuery { PageNumber = 1, PageSize = 10 };
        var result = await _service.GetByFacilityIdPagedAsync(facilityId, query);

        Assert.That(result.Items.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetMyBookingsAsync_ReturnsUserBookings()
    {
        Guid userId = Guid.NewGuid();
        var bookings = new List<Booking> { new() { UserId = userId } };
        _mockBookingRepo.Setup(b => b.GetByUserIdAsync(userId)).ReturnsAsync(bookings);

        var result = await _service.GetMyBookingsAsync(userId);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetByUserIdAsync_ReturnsUserBookings()
    {
        Guid userId = Guid.NewGuid();
        var bookings = new List<Booking> { new() { UserId = userId } };
        _mockBookingRepo.Setup(b => b.GetByUserIdAsync(userId)).ReturnsAsync(bookings);

        var result = await _service.GetByUserIdAsync(userId);

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void CreateAsync_ThrowsNotFound_WhenFacilityMissing()
    {
        var dto = new CreateBookingDto { FacilityId = Guid.NewGuid() };
        _mockFacilityRepo.Setup(f => f.GetByIdAsync(dto.FacilityId)).ReturnsAsync((Facility)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.CreateAsync(dto, Guid.NewGuid()));
    }

    [Test]
    public void CreateAsync_ThrowsBadRequest_WhenFacilityInactive()
    {
        var dto = new CreateBookingDto { FacilityId = Guid.NewGuid() };
        _mockFacilityRepo.Setup(f => f.GetByIdAsync(dto.FacilityId)).ReturnsAsync(new Facility { IsActive = false });

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CreateAsync(dto, Guid.NewGuid()));
    }

    [Test]
    public void CreateAsync_ThrowsBadRequest_WhenStartTimeAfterEndTime()
    {
        var dto = new CreateBookingDto
        {
            FacilityId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(1)
        };
        _mockFacilityRepo.Setup(f => f.GetByIdAsync(dto.FacilityId)).ReturnsAsync(new Facility { IsActive = true });

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CreateAsync(dto, Guid.NewGuid()));
    }

    [Test]
    public void CreateAsync_ThrowsBadRequest_WhenStartTimeInPast()
    {
        var dto = new CreateBookingDto
        {
            FacilityId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow.AddHours(1)
        };
        _mockFacilityRepo.Setup(f => f.GetByIdAsync(dto.FacilityId)).ReturnsAsync(new Facility { IsActive = true });

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CreateAsync(dto, Guid.NewGuid()));
    }

    [Test]
    public void CreateAsync_ThrowsConflict_WhenOverlappingBookingExists()
    {
        var dto = new CreateBookingDto
        {
            FacilityId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };
        _mockFacilityRepo.Setup(f => f.GetByIdAsync(dto.FacilityId)).ReturnsAsync(new Facility { IsActive = true });

        var overlapping = new List<Booking> { new() };
        _mockBookingRepo.Setup(b => b.GetByDateRangeAsync(dto.FacilityId, dto.StartTime, dto.EndTime)).ReturnsAsync(overlapping);

        Assert.ThrowsAsync<ConflictException>(async () => await _service.CreateAsync(dto, Guid.NewGuid()));
        _mockTransaction.Verify(t => t.RollbackAsync(default), Times.Once);
    }

    [Test]
    public async Task CreateAsync_HoldsBookingSuccessfully()
    {
        var dto = new CreateBookingDto
        {
            FacilityId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Date = DateTime.UtcNow.Date
        };
        var facility = new Facility { Id = dto.FacilityId, IsActive = true, HourlyRate = 100, Name = "Tennis Court" };
        _mockFacilityRepo.Setup(f => f.GetByIdAsync(dto.FacilityId)).ReturnsAsync(facility);

        _mockBookingRepo.Setup(b => b.GetByDateRangeAsync(dto.FacilityId, dto.StartTime, dto.EndTime)).ReturnsAsync(new List<Booking>());

        var userId = Guid.NewGuid();
        var result = await _service.CreateAsync(dto, userId);

        Assert.That(result.Status, Is.EqualTo(BookingStatus.Held));
        Assert.That(result.TotalCost, Is.EqualTo(100));
        _mockBookingRepo.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockTransaction.Verify(t => t.CommitAsync(default), Times.Once);
        _mockNotif.Verify(n => n.CreateAsync(userId, "Booking Held", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void CreatePaymentOrderAsync_ThrowsNotFound_WhenBookingMissing()
    {
        Guid id = Guid.NewGuid();
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync((Booking)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.CreatePaymentOrderAsync(id, Guid.NewGuid()));
    }

    [Test]
    public void CreatePaymentOrderAsync_ThrowsUnauthorized_WhenUserNotOwner()
    {
        Guid id = Guid.NewGuid();
        var booking = new Booking { Id = id, UserId = Guid.NewGuid(), Status = BookingStatus.Held, HoldExpiresAt = DateTime.UtcNow.AddMinutes(5) };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync(booking);

        Assert.ThrowsAsync<UnauthorizedException>(async () => await _service.CreatePaymentOrderAsync(id, Guid.NewGuid()));
    }

    [Test]
    public void CreatePaymentOrderAsync_ThrowsBadRequest_WhenNotHeld()
    {
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var booking = new Booking { Id = id, UserId = userId, Status = BookingStatus.Confirmed };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync(booking);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CreatePaymentOrderAsync(id, userId));
    }

    [Test]
    public void CreatePaymentOrderAsync_ThrowsBadRequest_WhenHoldExpired()
    {
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var booking = new Booking { Id = id, UserId = userId, Status = BookingStatus.Held, HoldExpiresAt = DateTime.UtcNow.AddMinutes(-1) };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync(booking);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CreatePaymentOrderAsync(id, userId));
        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Expired));
        _mockBookingRepo.Verify(b => b.UpdateAsync(booking), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task CreatePaymentOrderAsync_CreatesOrderSuccessfully()
    {
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var booking = new Booking { Id = id, UserId = userId, Status = BookingStatus.Held, HoldExpiresAt = DateTime.UtcNow.AddMinutes(5), TotalCost = 200 };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync(booking);

        var result = await _service.CreatePaymentOrderAsync(id, userId);

        Assert.That(result.OrderId, Does.StartWith("booking_order_"));
        Assert.That(result.Amount, Is.EqualTo(200));
        Assert.That(booking.TransactionRef, Is.EqualTo(result.OrderId));
        _mockBookingRepo.Verify(b => b.UpdateAsync(booking), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void VerifyPaymentAsync_ThrowsBadRequest_WhenSignatureInvalid()
    {
        var dto = new VerifyBookingPaymentDto { BookingId = Guid.NewGuid(), Signature = "invalid" };

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.VerifyPaymentAsync(dto));
    }

    [Test]
    public async Task VerifyPaymentAsync_ConfirmsBookingSuccessfully()
    {
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var booking = new Booking { Id = id, UserId = userId, Status = BookingStatus.Held, HoldExpiresAt = DateTime.UtcNow.AddMinutes(5) };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync(booking);

        var dto = new VerifyBookingPaymentDto { BookingId = id, OrderId = "order_123", PaymentId = "pay_123", Signature = "valid_sig" };

        var result = await _service.VerifyPaymentAsync(dto);

        Assert.That(result.Status, Is.EqualTo(BookingStatus.Confirmed));
        Assert.That(booking.TransactionRef, Is.EqualTo("pay_123"));
        _mockBookingRepo.Verify(b => b.UpdateAsync(booking), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockNotif.Verify(n => n.CreateAsync(userId, "Booking Confirmed", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void CancelAsync_ThrowsNotFound_WhenBookingMissing()
    {
        Guid id = Guid.NewGuid();
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync((Booking)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.CancelAsync(id, Guid.NewGuid()));
    }

    [Test]
    public void CancelAsync_ThrowsUnauthorized_WhenUserNotOwner()
    {
        Guid id = Guid.NewGuid();
        var booking = new Booking { Id = id, UserId = Guid.NewGuid() };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync(booking);

        Assert.ThrowsAsync<UnauthorizedException>(async () => await _service.CancelAsync(id, Guid.NewGuid()));
    }

    [Test]
    public void CancelAsync_ThrowsBadRequest_WhenAlreadyCancelledOrCompletedOrExpired()
    {
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var booking = new Booking { Id = id, UserId = userId, Status = BookingStatus.Cancelled };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync(booking);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CancelAsync(id, userId));

        booking.Status = BookingStatus.Completed;
        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CancelAsync(id, userId));

        booking.Status = BookingStatus.Expired;
        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CancelAsync(id, userId));
    }

    [Test]
    public async Task CancelAsync_CancelsBookingSuccessfully()
    {
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var booking = new Booking { Id = id, UserId = userId, Status = BookingStatus.Confirmed };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync(booking);

        await _service.CancelAsync(id, userId);

        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Cancelled));
        _mockBookingRepo.Verify(b => b.UpdateAsync(booking), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockNotif.Verify(n => n.CreateAsync(userId, "Booking Cancelled", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void CompleteAsync_ThrowsNotFound_WhenBookingMissing()
    {
        Guid id = Guid.NewGuid();
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync((Booking)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.CompleteAsync(id));
    }

    [Test]
    public void CompleteAsync_ThrowsBadRequest_WhenNotConfirmed()
    {
        Guid id = Guid.NewGuid();
        var booking = new Booking { Id = id, Status = BookingStatus.Held };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync(booking);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CompleteAsync(id));
    }

    [Test]
    public async Task CompleteAsync_CompletesBookingSuccessfully()
    {
        Guid id = Guid.NewGuid();
        var booking = new Booking { Id = id, Status = BookingStatus.Confirmed };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(id)).ReturnsAsync(booking);

        await _service.CompleteAsync(id);

        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Completed));
        _mockBookingRepo.Verify(b => b.UpdateAsync(booking), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task ExpireHoldsAsync_ExpiresAllOverdueHolds()
    {
        var booking1 = new Booking { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        var booking2 = new Booking { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        var expired = new List<Booking> { booking1, booking2 };

        _mockBookingRepo.Setup(b => b.GetExpiredHoldsAsync()).ReturnsAsync(expired);

        await _service.ExpireHoldsAsync();

        Assert.That(booking1.Status, Is.EqualTo(BookingStatus.Expired));
        Assert.That(booking2.Status, Is.EqualTo(BookingStatus.Expired));
        _mockBookingRepo.Verify(b => b.UpdateAsync(booking1), Times.Once);
        _mockBookingRepo.Verify(b => b.UpdateAsync(booking2), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockNotif.Verify(n => n.CreateAsync(It.IsAny<Guid>(), "Booking Expired", It.IsAny<string>()), Times.Exactly(2));
    }
}
