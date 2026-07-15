using Microsoft.Extensions.Logging;
using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;
    private readonly BillService _billService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IUnitOfWork uow,
        INotificationService notificationService,
        BillService billService,
        ILogger<PaymentService> logger)
    {
        _uow = uow;
        _notificationService = notificationService;
        _billService = billService;
        _logger = logger;
    }

    public async Task<CreatePaymentOrderResponseDto> CreateOrderAsync(
        Guid billId,
        Guid userId)
    {
        _logger.LogInformation(
            "Creating simulated payment order for BillId {BillId} by UserId {UserId}",
            billId,
            userId);

        var bill = await _uow.Bills.GetByIdAsync(billId)
            ?? throw new NotFoundException("Bill", billId);

        if (bill.BilledToUserId != userId)
        {
            _logger.LogWarning(
                "UserId {UserId} attempted to create a payment order for BillId {BillId} belonging to UserId {BilledToUserId}",
                userId,
                billId,
                bill.BilledToUserId);

            throw new UnauthorizedException(
                "You are not authorized to pay this bill.");
        }

        if (bill.Status == BillingStatus.Paid)
        {
            throw new BadRequestException(
                "This bill has already been paid.");
        }

        if (bill.Status == BillingStatus.Disputed)
        {
            throw new BadRequestException(
                "This bill is under dispute and cannot be paid.");
        }

        if (bill.Status == BillingStatus.Processing &&
            !string.IsNullOrWhiteSpace(bill.TransactionRef))
        {
            _logger.LogInformation(
                "Returning existing simulated payment order {OrderId} for BillId {BillId}",
                bill.TransactionRef,
                bill.Id);

            return new CreatePaymentOrderResponseDto
            {
                OrderId = bill.TransactionRef,
                Amount = bill.Total,
                Currency = "INR"
            };
        }

        var simulatedOrderId = $"sim_order_{Guid.NewGuid():N}";

        bill.Status = BillingStatus.Processing;
        bill.TransactionRef = simulatedOrderId;
        bill.UpdatedAt = DateTime.UtcNow;

        await _uow.Bills.UpdateAsync(bill);
        await _uow.SaveChangesAsync();

        _logger.LogInformation(
            "Created simulated payment order {OrderId} for BillId {BillId}",
            simulatedOrderId,
            bill.Id);

        return new CreatePaymentOrderResponseDto
        {
            OrderId = simulatedOrderId,
            Amount = bill.Total,
            Currency = "INR"
        };
    }

    public async Task<BillResponseDto> CompleteSimulatedPaymentAsync(
        CompletePaymentDto dto,
        Guid userId)
    {
        _logger.LogInformation(
            "Attempting to complete simulated payment for OrderId {OrderId} by UserId {UserId}",
            dto.OrderId,
            userId);

        var allBills = await _uow.Bills.GetAllAsync();

        var bill = allBills.FirstOrDefault(
            b => b.TransactionRef == dto.OrderId);

        if (bill is null)
        {
            _logger.LogWarning(
                "No bill found for simulated payment OrderId {OrderId}",
                dto.OrderId);

            throw new NotFoundException(
                "No bill found for this simulated payment order.");
        }

        if (bill.BilledToUserId != userId)
        {
            _logger.LogWarning(
                "UserId {UserId} attempted to complete OrderId {OrderId} for BillId {BillId} belonging to UserId {BilledToUserId}",
                userId,
                dto.OrderId,
                bill.Id,
                bill.BilledToUserId);

            throw new UnauthorizedException(
                "You are not authorized to complete this payment.");
        }

        if (bill.Status != BillingStatus.Processing)
        {
            _logger.LogWarning(
                "Simulated payment OrderId {OrderId} cannot be completed because BillId {BillId} has status {Status}",
                dto.OrderId,
                bill.Id,
                bill.Status);

            throw new BadRequestException(
                "This bill does not have a payment currently being processed.");
        }

        var simulatedPaymentId =
            $"sim_payment_{Guid.NewGuid():N}";

        bill.Status = BillingStatus.Paid;
        bill.PaidAt = DateTime.UtcNow;
        bill.TransactionRef = simulatedPaymentId;
        bill.UpdatedAt = DateTime.UtcNow;

        await _uow.Bills.UpdateAsync(bill);
        await _uow.SaveChangesAsync();

        _logger.LogInformation(
            "Simulated payment completed successfully for BillId {BillId}. PaymentReference {PaymentReference}",
            bill.Id,
            simulatedPaymentId);

        await _notificationService.CreateAsync(
            bill.BilledToUserId,
            "Payment Successful",
            $"Maintenance bill for {bill.Period} paid successfully. Ref: {simulatedPaymentId}.");

        return await _billService.MapToDtoAsync(bill);
    }
}