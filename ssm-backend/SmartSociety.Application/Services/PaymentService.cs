using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Repository.Interfaces;
using Microsoft.Extensions.Configuration;

namespace SmartSociety.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;
    private readonly INotificationService _notificationService;
    private readonly BillService _billService;

    public PaymentService(
        IUnitOfWork uow,
        IConfiguration config,
        INotificationService notificationService,
        BillService billService)
    {
        _uow = uow;
        _config = config;
        _notificationService = notificationService;
        _billService = billService;
    }

    public async Task<CreatePaymentOrderResponseDto> CreateOrderAsync(Guid billId, Guid userId)
    {
        var bill = await _uow.Bills.GetByIdAsync(billId)
            ?? throw new NotFoundException("Bill", billId);

        // Caller must be the person billed (either resident or owner)
        if (bill.BilledToUserId != userId)
            throw new UnauthorizedException("You are not authorized to pay this bill.");

        if (bill.Status == BillingStatus.Paid)
            throw new BadRequestException("This bill has already been paid.");

        if (bill.Status == BillingStatus.Disputed)
            throw new BadRequestException("This bill is under dispute and cannot be paid.");

        var dummyOrderId = $"order_dummy_{Guid.NewGuid():N}";

        bill.Status = BillingStatus.Processing;
        bill.TransactionRef = dummyOrderId;
        bill.UpdatedAt = DateTime.UtcNow;

        await _uow.Bills.UpdateAsync(bill);
        await _uow.SaveChangesAsync();

        return new CreatePaymentOrderResponseDto
        {
            OrderId = dummyOrderId,
            Amount = bill.Total,
            Currency = "INR"
        };
    }

    public async Task<BillResponseDto> VerifyAndCompleteAsync(VerifyPaymentDto dto)
    {
        if (dto.Signature != "valid_sig")
            throw new BadRequestException("Payment verification failed. Invalid signature.");

        var allBills = await _uow.Bills.GetAllAsync();
        var bill = allBills.FirstOrDefault(b => b.TransactionRef == dto.OrderId)
            ?? throw new NotFoundException("No bill found for this payment order.");

        bill.Status = BillingStatus.Paid;
        bill.PaidAt = DateTime.UtcNow;
        bill.TransactionRef = dto.PaymentId;
        bill.UpdatedAt = DateTime.UtcNow;

        await _uow.Bills.UpdateAsync(bill);
        await _uow.SaveChangesAsync();

        // Notify the person who was billed
        await _notificationService.CreateAsync(
            bill.BilledToUserId,
            "Payment Successful",
            $"Maintenance bill for {bill.Period} paid successfully. Ref: {bill.TransactionRef}.");

        return await _billService.MapToDtoAsync(bill);
    }
}