using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Services;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Tests.Services;

[TestFixture]
public class PaymentServiceTest
{
    private Mock<IUnitOfWork> _mockUow;
    private Mock<IBillRepository> _mockBillRepo;
    private Mock<IApartmentRepository> _mockApartmentRepo;
    private Mock<IResidentRepository> _mockResidentRepo;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<INotificationService> _mockNotif;
    private Mock<IConfiguration> _mockConfig;
    private PaymentService _service;

    [SetUp]
    public void Setup()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockBillRepo = new Mock<IBillRepository>();
        _mockApartmentRepo = new Mock<IApartmentRepository>();
        _mockResidentRepo = new Mock<IResidentRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockNotif = new Mock<INotificationService>();
        _mockConfig = new Mock<IConfiguration>();

        _mockUow.Setup(u => u.Bills).Returns(_mockBillRepo.Object);
        _mockUow.Setup(u => u.Apartments).Returns(_mockApartmentRepo.Object);
        _mockUow.Setup(u => u.Residents).Returns(_mockResidentRepo.Object);
        _mockUow.Setup(u => u.Users).Returns(_mockUserRepo.Object);

        var billService = new BillService(_mockUow.Object, _mockNotif.Object);
        _service = new PaymentService(_mockUow.Object, _mockConfig.Object, _mockNotif.Object, billService);
    }

    [Test]
    public void CreateOrderAsync_ThrowsNotFound_WhenBillMissing()
    {
        Guid billId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        _mockBillRepo.Setup(r => r.GetByIdAsync(billId)).ReturnsAsync((Bill)null!);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.CreateOrderAsync(billId, userId));
    }

    [Test]
    public void CreateOrderAsync_ThrowsUnauthorized_WhenNotBilledUser()
    {
        Guid billId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var bill = new Bill { Id = billId, BilledToUserId = Guid.NewGuid(), Status = BillingStatus.Unpaid };
        _mockBillRepo.Setup(r => r.GetByIdAsync(billId)).ReturnsAsync(bill);

        Assert.ThrowsAsync<UnauthorizedException>(async () => await _service.CreateOrderAsync(billId, userId));
    }

    [Test]
    public void CreateOrderAsync_ThrowsBadRequest_WhenBillAlreadyPaid()
    {
        Guid billId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var bill = new Bill { Id = billId, BilledToUserId = userId, Status = BillingStatus.Paid };
        _mockBillRepo.Setup(r => r.GetByIdAsync(billId)).ReturnsAsync(bill);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CreateOrderAsync(billId, userId));
    }

    [Test]
    public void CreateOrderAsync_ThrowsBadRequest_WhenBillDisputed()
    {
        Guid billId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var bill = new Bill { Id = billId, BilledToUserId = userId, Status = BillingStatus.Disputed };
        _mockBillRepo.Setup(r => r.GetByIdAsync(billId)).ReturnsAsync(bill);

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.CreateOrderAsync(billId, userId));
    }

    [Test]
    public async Task CreateOrderAsync_CreatesOrderSuccessfully()
    {
        Guid billId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var bill = new Bill { Id = billId, BilledToUserId = userId, Status = BillingStatus.Unpaid, BaseAmount = 1000, Penalty = 100 };
        _mockBillRepo.Setup(r => r.GetByIdAsync(billId)).ReturnsAsync(bill);

        var result = await _service.CreateOrderAsync(billId, userId);

        Assert.That(result.OrderId, Does.StartWith("order_dummy_"));
        Assert.That(result.Amount, Is.EqualTo(1100)); // Total (Base + Penalty)
        Assert.That(bill.Status, Is.EqualTo(BillingStatus.Processing));
        Assert.That(bill.TransactionRef, Is.EqualTo(result.OrderId));
        _mockBillRepo.Verify(r => r.UpdateAsync(bill), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void VerifyAndCompleteAsync_ThrowsBadRequest_WhenSignatureInvalid()
    {
        var dto = new VerifyPaymentDto { OrderId = "order_123", PaymentId = "pay_123", Signature = "invalid" };

        Assert.ThrowsAsync<BadRequestException>(async () => await _service.VerifyAndCompleteAsync(dto));
    }

    [Test]
    public void VerifyAndCompleteAsync_ThrowsNotFound_WhenBillNotFoundByOrderId()
    {
        var dto = new VerifyPaymentDto { OrderId = "order_123", PaymentId = "pay_123", Signature = "valid_sig" };
        var bills = new List<Bill> { new() { TransactionRef = "different_order" } };
        _mockBillRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(bills);

        Assert.ThrowsAsync<NotFoundException>(async () => await _service.VerifyAndCompleteAsync(dto));
    }

    [Test]
    public async Task VerifyAndCompleteAsync_CompletesPaymentSuccessfully()
    {
        var dto = new VerifyPaymentDto { OrderId = "order_123", PaymentId = "pay_123", Signature = "valid_sig" };
        Guid apartmentId = Guid.NewGuid();
        Guid billedToUserId = Guid.NewGuid();
        var bill = new Bill { Id = Guid.NewGuid(), ApartmentId = apartmentId, BilledToUserId = billedToUserId, Period = "12-2026", TransactionRef = "order_123", Status = BillingStatus.Processing };
        var bills = new List<Bill> { bill };
        _mockBillRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(bills);

        // MapToDto lookup setup
        _mockBillRepo.Setup(r => r.GetByIdAsync(bill.Id)).ReturnsAsync(bill);
        _mockApartmentRepo.Setup(a => a.GetByIdAsync(apartmentId)).ReturnsAsync(new Apartment { Block = "A", Number = "101" });

        var result = await _service.VerifyAndCompleteAsync(dto);

        Assert.That(result.Status, Is.EqualTo(BillingStatus.Paid));
        Assert.That(result.TransactionRef, Is.EqualTo("pay_123"));
        Assert.That(bill.PaidAt, Is.Not.Null);

        _mockBillRepo.Verify(r => r.UpdateAsync(bill), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockNotif.Verify(n => n.CreateAsync(billedToUserId, "Payment Successful", It.IsAny<string>()), Times.Once);
    }
}
