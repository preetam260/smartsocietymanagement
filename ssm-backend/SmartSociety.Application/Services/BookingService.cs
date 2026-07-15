using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace SmartSociety.Application.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;
    private readonly ILogger<BookingService> _logger;

    public BookingService(IUnitOfWork uow, INotificationService notificationService, ILogger<BookingService> logger)
    {
        _uow = uow;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<IEnumerable<BookingResponseDto>> GetAllAsync()
    {
        var bookings = await _uow.Bookings.GetAllAsync();
        return await MapToDtoListAsync(bookings);
    }

    public async Task<PagedResult<BookingResponseDto>> GetAllPagedAsync(PaginationQuery query)
    {
        var bookings = await _uow.Bookings.GetAllAsync();
        var dtos = await MapToDtoListAsync(bookings);
        return PagedResult<BookingResponseDto>.Create(dtos, query.PageNumber, query.PageSize, query.Search,
            new Func<BookingResponseDto, string?>[] { b => b.UserName, b => b.FacilityName });
    }

    public async Task<BookingResponseDto> GetByIdAsync(Guid id)
    {
        var booking = await _uow.Bookings.GetByIdAsync(id)
            ?? throw new NotFoundException("Booking", id);
        return await MapToDtoAsync(booking);
    }

    public async Task<IEnumerable<BookingResponseDto>> GetByFacilityIdAsync(Guid facilityId)
    {
        var bookings = await _uow.Bookings.GetAllBookingsByFacilityAsync(facilityId);
        return await MapToDtoListAsync(bookings);
    }

    public async Task<PagedResult<BookingResponseDto>> GetByFacilityIdPagedAsync(Guid facilityId, PaginationQuery query)
    {
        var bookings = await _uow.Bookings.GetAllBookingsByFacilityAsync(facilityId);
        var dtos = await MapToDtoListAsync(bookings);
        return PagedResult<BookingResponseDto>.Create(dtos, query.PageNumber, query.PageSize, query.Search,
            new Func<BookingResponseDto, string?>[] { b => b.UserName, b => b.FacilityName });
    }

    public async Task<IEnumerable<BookingResponseDto>> GetMyBookingsAsync(Guid userId)
    {
        var bookings = await _uow.Bookings.GetByUserIdAsync(userId);
        return await MapToDtoListAsync(bookings);
    }

    public async Task<IEnumerable<BookingResponseDto>> GetByUserIdAsync(Guid userId)
    {
        var bookings = await _uow.Bookings.GetByUserIdAsync(userId);
        return await MapToDtoListAsync(bookings);
    }

    public async Task<BookingResponseDto> CreateAsync(CreateBookingDto dto, Guid userId)
    {
        var facility = await _uow.Facilities.GetByIdAsync(dto.FacilityId)
            ?? throw new NotFoundException("Facility", dto.FacilityId);

        var resident = await _uow.Residents.GetCurrentByUserIdAsync(userId);
        if (resident == null)
            throw new UnauthorizedException("You must be an active resident to book facilities.");

        if (!facility.IsActive)
            throw new BadRequestException("This facility is not currently available for booking.");

        if (dto.StartTime >= dto.EndTime)
            throw new BadRequestException("Start time must be before end time.");

        if (dto.StartTime <= DateTime.UtcNow)
            throw new BadRequestException("Booking must be scheduled for a future time.");



        await using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            var overlapping = await _uow.Bookings.GetByDateRangeAsync(dto.FacilityId, dto.StartTime, dto.EndTime);
            if (overlapping.Any())
                throw new ConflictException("This facility is already booked for the selected time slot.");

            var hours = (decimal)(dto.EndTime - dto.StartTime).TotalHours;
            var totalCost = Math.Round(facility.HourlyRate * hours, 2);

            var booking = new Booking
            {
                FacilityId = dto.FacilityId,
                UserId = userId,
                Date = dto.StartTime.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                TotalCost = totalCost,
                Status = BookingStatus.Held,
                HoldExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            await _uow.Bookings.AddAsync(booking);
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();

            await _notificationService.CreateAsync(
                userId,
                "Booking Held",
                $"{facility.Name} slot held on {dto.Date:dd MMM yyyy} from {dto.StartTime:hh:mm tt} to {dto.EndTime:hh:mm tt}. Total: ₹{totalCost}. Complete payment within 10 minutes to confirm.");

            return await MapToDtoAsync(booking);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<CreatePaymentOrderResponseDto> CreatePaymentOrderAsync(Guid bookingId, Guid userId)
    {
        _logger.LogInformation(
            "UserId {UserId} is creating a simulated payment order for BookingId {BookingId}",
            userId,
            bookingId);

        var booking = await _uow.Bookings.GetByIdAsync(bookingId)
            ?? throw new NotFoundException("Booking", bookingId);

        if (booking.UserId != userId)
        {
            _logger.LogWarning(
                "UserId {UserId} attempted to create a payment order for BookingId {BookingId} owned by UserId {BookingOwnerId}",
                userId,
                bookingId,
                booking.UserId);

            throw new UnauthorizedException(
                "You are not authorized to pay for this booking.");
        }

        if (booking.Status != BookingStatus.Held)
        {
            throw new BadRequestException(
                "Only held bookings can be paid for.");
        }

        if (booking.HoldExpiresAt == null)
        {
            _logger.LogWarning(
                "BookingId {BookingId} is Held but has no HoldExpiresAt value",
                bookingId);

            throw new BadRequestException(
                "This booking does not have a valid payment hold.");
        }

        if (booking.HoldExpiresAt <= DateTime.UtcNow)
        {
            booking.Status = BookingStatus.Expired;
            booking.UpdatedAt = DateTime.UtcNow;

            await _uow.Bookings.UpdateAsync(booking);
            await _uow.SaveChangesAsync();

            _logger.LogInformation(
                "BookingId {BookingId} expired before payment order creation",
                bookingId);

            throw new BadRequestException(
                "This booking hold has expired. Please create a new booking.");
        }

        // If an order has already been created for this held booking,
        // return the same order instead of generating another one.
        if (!string.IsNullOrWhiteSpace(booking.TransactionRef) &&
            booking.TransactionRef.StartsWith(
                "sim_booking_order_",
                StringComparison.Ordinal))
        {
            _logger.LogInformation(
                "Returning existing simulated payment OrderId {OrderId} for BookingId {BookingId}",
                booking.TransactionRef,
                bookingId);

            return new CreatePaymentOrderResponseDto
            {
                OrderId = booking.TransactionRef,
                Amount = booking.TotalCost,
                Currency = "INR"
            };
        }

        var simulatedOrderId =
            $"sim_booking_order_{Guid.NewGuid():N}";

        booking.TransactionRef = simulatedOrderId;
        booking.UpdatedAt = DateTime.UtcNow;

        await _uow.Bookings.UpdateAsync(booking);
        await _uow.SaveChangesAsync();

        _logger.LogInformation(
            "Created simulated payment OrderId {OrderId} for BookingId {BookingId}",
            simulatedOrderId,
            bookingId);

        return new CreatePaymentOrderResponseDto
        {
            OrderId = simulatedOrderId,
            Amount = booking.TotalCost,
            Currency = "INR"
        };
    }
    public async Task<BookingResponseDto> CompleteSimulatedPaymentAsync(Guid bookingId, CompleteBookingPaymentDto dto, Guid userId)
    {
        _logger.LogInformation(
            "UserId {UserId} is attempting to complete simulated payment for BookingId {BookingId} using OrderId {OrderId}",
            userId,
            bookingId,
            dto.OrderId);

        var booking = await _uow.Bookings.GetByIdAsync(bookingId)
            ?? throw new NotFoundException("Booking", bookingId);

        if (booking.UserId != userId)
        {
            _logger.LogWarning(
                "UserId {UserId} attempted to complete payment for BookingId {BookingId} owned by UserId {BookingOwnerId}",
                userId,
                bookingId,
                booking.UserId);

            throw new UnauthorizedException(
                "You are not authorized to complete payment for this booking.");
        }

        if (booking.Status != BookingStatus.Held)
        {
            _logger.LogWarning(
                "Payment completion rejected for BookingId {BookingId}. Current status is {Status}",
                bookingId,
                booking.Status);

            throw new BadRequestException(
                "Only held bookings can be confirmed through payment.");
        }

        if (booking.HoldExpiresAt == null)
        {
            _logger.LogWarning(
                "BookingId {BookingId} is Held but has no HoldExpiresAt value",
                bookingId);

            throw new BadRequestException(
                "This booking does not have a valid payment hold.");
        }

        if (booking.HoldExpiresAt <= DateTime.UtcNow)
        {
            booking.Status = BookingStatus.Expired;
            booking.UpdatedAt = DateTime.UtcNow;

            await _uow.Bookings.UpdateAsync(booking);
            await _uow.SaveChangesAsync();

            _logger.LogInformation(
                "BookingId {BookingId} expired before simulated payment could be completed",
                bookingId);

            throw new BadRequestException(
                "This booking hold has expired. Please create a new booking.");
        }

        if (string.IsNullOrWhiteSpace(booking.TransactionRef) ||
            !string.Equals(
                booking.TransactionRef,
                dto.OrderId,
                StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "Payment OrderId mismatch for BookingId {BookingId}. Submitted OrderId {SubmittedOrderId}",
                bookingId,
                dto.OrderId);

            throw new BadRequestException(
                "The payment order does not match this booking.");
        }

        if (!dto.OrderId.StartsWith(
                "sim_booking_order_",
                StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "Invalid simulated booking OrderId format submitted for BookingId {BookingId}",
                bookingId);

            throw new BadRequestException(
                "Invalid simulated payment order.");
        }

        var simulatedPaymentId =
            $"sim_booking_payment_{Guid.NewGuid():N}";

        booking.Status = BookingStatus.Confirmed;
        booking.TransactionRef = simulatedPaymentId;
        booking.UpdatedAt = DateTime.UtcNow;

        await _uow.Bookings.UpdateAsync(booking);
        await _uow.SaveChangesAsync();

        _logger.LogInformation(
            "Simulated payment completed successfully for BookingId {BookingId}. PaymentReference {PaymentReference}",
            bookingId,
            simulatedPaymentId);

        var facility =
            await _uow.Facilities.GetByIdAsync(
                booking.FacilityId);

        await _notificationService.CreateAsync(
            booking.UserId,
            "Booking Confirmed",
            $"{facility?.Name ?? "Facility"} booked on " +
            $"{booking.Date:dd MMM yyyy} from " +
            $"{booking.StartTime:hh:mm tt} to " +
            $"{booking.EndTime:hh:mm tt}. " +
            $"Total: ₹{booking.TotalCost}. " +
            $"Ref: {simulatedPaymentId}.");

        return await MapToDtoAsync(booking);
    }

    public async Task CancelAsync(Guid id, Guid userId)
    {
        var booking = await _uow.Bookings.GetByIdAsync(id)
            ?? throw new NotFoundException("Booking", id);

        if (booking.UserId != userId)
            throw new UnauthorizedException("You are not authorized to cancel this booking.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new BadRequestException("This booking is already cancelled.");

        if (booking.Status == BookingStatus.Completed)
            throw new BadRequestException("Cannot cancel a completed booking.");

        if (booking.Status == BookingStatus.Expired)
            throw new BadRequestException("Cannot cancel an expired booking.");

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        await _uow.Bookings.UpdateAsync(booking);
        await _uow.SaveChangesAsync();

        await _notificationService.CreateAsync(
            userId,
            "Booking Cancelled",
            $"Your booking (ID: {id}) has been cancelled. No refunds are applicable.");
    }

    public async Task CompleteAsync(Guid id)
    {
        var booking = await _uow.Bookings.GetByIdAsync(id)
            ?? throw new NotFoundException("Booking", id);

        if (booking.Status != BookingStatus.Confirmed)
            throw new BadRequestException("Only confirmed bookings can be marked as completed.");

        booking.Status = BookingStatus.Completed;
        booking.UpdatedAt = DateTime.UtcNow;

        await _uow.Bookings.UpdateAsync(booking);
        await _uow.SaveChangesAsync();
    }

    public async Task ExpireHoldsAsync()
    {
        var now = DateTime.UtcNow;

        _logger.LogDebug("Searching for expired booking holds at {CurrentUtcTime}", now);

        var bookings = await _uow.Bookings.GetAllAsync();

        var expiredHolds = bookings
            .Where(b =>
                b.Status == BookingStatus.Held &&
                b.HoldExpiresAt.HasValue &&
                b.HoldExpiresAt.Value <= now)
            .ToList();

        if (expiredHolds.Count == 0)
        {
            _logger.LogDebug(
                "No expired booking holds found.");

            return;
        }

        _logger.LogInformation("Found {ExpiredHoldCount} expired booking holds.", expiredHolds.Count);

        foreach (var booking in expiredHolds)
        {
            booking.Status = BookingStatus.Expired;
            booking.UpdatedAt = now;

            await _uow.Bookings.UpdateAsync(booking);

            _logger.LogInformation(
                "BookingId {BookingId} expired. " +
                "Hold expired at {HoldExpiresAtUtc}",
                booking.Id,
                booking.HoldExpiresAt);
        }

        await _uow.SaveChangesAsync();

        _logger.LogInformation(
            "Successfully expired {ExpiredHoldCount} booking holds.",
            expiredHolds.Count);
    }
    
    private async Task<BookingResponseDto> MapToDtoAsync(Booking b)
    {
        var facility = await _uow.Facilities.GetByIdAsync(b.FacilityId);
        var user = await _uow.Users.GetByIdAsync(b.UserId);
        return new BookingResponseDto
        {
            Id = b.Id,
            FacilityId = b.FacilityId,
            FacilityName = facility?.Name ?? "",
            UserId = b.UserId,
            UserName = user?.Name ?? "",
            Date = b.Date,
            StartTime = b.StartTime,
            EndTime = b.EndTime,
            TotalCost = b.TotalCost,
            Status = b.Status,
            HoldExpiresAt = b.HoldExpiresAt,
            TransactionRef = b.TransactionRef
        };
    }

    private async Task<IEnumerable<BookingResponseDto>> MapToDtoListAsync(IEnumerable<Booking> bookings)
    {
        var result = new List<BookingResponseDto>();
        foreach (var b in bookings)
            result.Add(await MapToDtoAsync(b));
        return result;
    }
}