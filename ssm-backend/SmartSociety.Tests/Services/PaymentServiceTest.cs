using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Application.Services;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class PaymentServiceTest
{
    private Mock<IUnitOfWork> _mockUow = null!;
    private Mock<IBillRepository> _mockBillRepo = null!;
    private Mock<IApartmentRepository> _mockApartmentRepo = null!;
    private Mock<IResidentRepository> _mockResidentRepo = null!;
    private Mock<IUserRepository> _mockUserRepo = null!;
    private Mock<INotificationService> _mockNotif = null!;
    private Mock<ILogger<PaymentService>> _mockLogger = null!;

    private PaymentService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockBillRepo = new Mock<IBillRepository>();
        _mockApartmentRepo = new Mock<IApartmentRepository>();
        _mockResidentRepo = new Mock<IResidentRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockNotif = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<PaymentService>>();

        _mockUow
            .Setup(u => u.Bills)
            .Returns(_mockBillRepo.Object);

        _mockUow
            .Setup(u => u.Apartments)
            .Returns(_mockApartmentRepo.Object);

        _mockUow
            .Setup(u => u.Residents)
            .Returns(_mockResidentRepo.Object);

        _mockUow
            .Setup(u => u.Users)
            .Returns(_mockUserRepo.Object);

        var billService = new BillService(
            _mockUow.Object,
            _mockNotif.Object);

        _service = new PaymentService(
            _mockUow.Object,
            _mockNotif.Object,
            billService,
            _mockLogger.Object);
    }

    [Test]
    public void CreateOrderAsync_ThrowsNotFound_WhenBillMissing()
    {
        var billId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockBillRepo
            .Setup(r => r.GetByIdAsync(billId))
            .ReturnsAsync((Bill)null!);

        Assert.ThrowsAsync<NotFoundException>(
            async () =>
                await _service.CreateOrderAsync(
                    billId,
                    userId));
    }

    [Test]
    public void CreateOrderAsync_ThrowsUnauthorized_WhenNotBilledUser()
    {
        var billId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var bill = new Bill
        {
            Id = billId,
            BilledToUserId = Guid.NewGuid(),
            Status = BillingStatus.Unpaid
        };

        _mockBillRepo
            .Setup(r => r.GetByIdAsync(billId))
            .ReturnsAsync(bill);

        Assert.ThrowsAsync<UnauthorizedException>(
            async () =>
                await _service.CreateOrderAsync(
                    billId,
                    userId));
    }

    [Test]
    public void CreateOrderAsync_ThrowsBadRequest_WhenBillAlreadyPaid()
    {
        var billId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var bill = new Bill
        {
            Id = billId,
            BilledToUserId = userId,
            Status = BillingStatus.Paid
        };

        _mockBillRepo
            .Setup(r => r.GetByIdAsync(billId))
            .ReturnsAsync(bill);

        Assert.ThrowsAsync<BadRequestException>(
            async () =>
                await _service.CreateOrderAsync(
                    billId,
                    userId));
    }

    [Test]
    public void CreateOrderAsync_ThrowsBadRequest_WhenBillDisputed()
    {
        var billId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var bill = new Bill
        {
            Id = billId,
            BilledToUserId = userId,
            Status = BillingStatus.Disputed
        };

        _mockBillRepo
            .Setup(r => r.GetByIdAsync(billId))
            .ReturnsAsync(bill);

        Assert.ThrowsAsync<BadRequestException>(
            async () =>
                await _service.CreateOrderAsync(
                    billId,
                    userId));
    }

    [Test]
    public async Task CreateOrderAsync_CreatesSimulatedOrderSuccessfully()
    {
        var billId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var bill = new Bill
        {
            Id = billId,
            BilledToUserId = userId,
            Status = BillingStatus.Unpaid,
            BaseAmount = 1000,
            Penalty = 100
        };

        _mockBillRepo
            .Setup(r => r.GetByIdAsync(billId))
            .ReturnsAsync(bill);

        var result = await _service.CreateOrderAsync(
            billId,
            userId);

        Assert.That(
            result.OrderId,
            Does.StartWith("sim_order_"));

        Assert.That(
            result.Amount,
            Is.EqualTo(1100));

        Assert.That(
            bill.Status,
            Is.EqualTo(BillingStatus.Processing));

        Assert.That(
            bill.TransactionRef,
            Is.EqualTo(result.OrderId));

        _mockBillRepo.Verify(
            r => r.UpdateAsync(bill),
            Times.Once);

        _mockUow.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Test]
    public async Task CreateOrderAsync_ReturnsExistingOrder_WhenAlreadyProcessing()
    {
        var billId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var bill = new Bill
        {
            Id = billId,
            BilledToUserId = userId,
            Status = BillingStatus.Processing,
            TransactionRef = "sim_order_existing",
            BaseAmount = 1000,
            Penalty = 100
        };

        _mockBillRepo
            .Setup(r => r.GetByIdAsync(billId))
            .ReturnsAsync(bill);

        var result = await _service.CreateOrderAsync(
            billId,
            userId);

        Assert.That(
            result.OrderId,
            Is.EqualTo("sim_order_existing"));

        _mockBillRepo.Verify(
            r => r.UpdateAsync(It.IsAny<Bill>()),
            Times.Never);

        _mockUow.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Test]
    public void CompleteSimulatedPaymentAsync_ThrowsNotFound_WhenOrderDoesNotExist()
    {
        var dto = new CompletePaymentDto
        {
            OrderId = "sim_order_missing"
        };

        _mockBillRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Bill>());

        Assert.ThrowsAsync<NotFoundException>(
            async () =>
                await _service.CompleteSimulatedPaymentAsync(
                    dto,
                    Guid.NewGuid()));
    }

    [Test]
    public void CompleteSimulatedPaymentAsync_ThrowsUnauthorized_WhenOrderBelongsToAnotherUser()
    {
        var ownerUserId = Guid.NewGuid();
        var attackingUserId = Guid.NewGuid();

        var bill = new Bill
        {
            Id = Guid.NewGuid(),
            BilledToUserId = ownerUserId,
            Status = BillingStatus.Processing,
            TransactionRef = "sim_order_123"
        };

        _mockBillRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Bill> { bill });

        var dto = new CompletePaymentDto
        {
            OrderId = "sim_order_123"
        };

        Assert.ThrowsAsync<UnauthorizedException>(
            async () =>
                await _service.CompleteSimulatedPaymentAsync(
                    dto,
                    attackingUserId));

        Assert.That(
            bill.Status,
            Is.EqualTo(BillingStatus.Processing));

        _mockBillRepo.Verify(
            r => r.UpdateAsync(It.IsAny<Bill>()),
            Times.Never);

        _mockUow.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Test]
    public void CompleteSimulatedPaymentAsync_ThrowsBadRequest_WhenBillIsNotProcessing()
    {
        var userId = Guid.NewGuid();

        var bill = new Bill
        {
            Id = Guid.NewGuid(),
            BilledToUserId = userId,
            Status = BillingStatus.Unpaid,
            TransactionRef = "sim_order_123"
        };

        _mockBillRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Bill> { bill });

        var dto = new CompletePaymentDto
        {
            OrderId = "sim_order_123"
        };

        Assert.ThrowsAsync<BadRequestException>(
            async () =>
                await _service.CompleteSimulatedPaymentAsync(
                    dto,
                    userId));
    }

    [Test]
    public async Task CompleteSimulatedPaymentAsync_CompletesPaymentSuccessfully()
    {
        var apartmentId = Guid.NewGuid();
        var billedToUserId = Guid.NewGuid();

        var bill = new Bill
        {
            Id = Guid.NewGuid(),
            ApartmentId = apartmentId,
            BilledToUserId = billedToUserId,
            Period = "12-2026",
            TransactionRef = "sim_order_123",
            Status = BillingStatus.Processing
        };

        _mockBillRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Bill> { bill });

        _mockBillRepo
            .Setup(r => r.GetByIdAsync(bill.Id))
            .ReturnsAsync(bill);

        _mockApartmentRepo
            .Setup(a => a.GetByIdAsync(apartmentId))
            .ReturnsAsync(
                new Apartment
                {
                    Block = "A",
                    Number = "101"
                });

        var dto = new CompletePaymentDto
        {
            OrderId = "sim_order_123"
        };

        var result =
            await _service.CompleteSimulatedPaymentAsync(
                dto,
                billedToUserId);

        Assert.That(
            result.Status,
            Is.EqualTo(BillingStatus.Paid));

        Assert.That(
            result.TransactionRef,
            Does.StartWith("sim_payment_"));

        Assert.That(
            bill.PaidAt,
            Is.Not.Null);

        _mockBillRepo.Verify(
            r => r.UpdateAsync(bill),
            Times.Once);

        _mockUow.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);

        _mockNotif.Verify(
            n => n.CreateAsync(
                billedToUserId,
                "Payment Successful",
                It.IsAny<string>()),
            Times.Once);
    }
}