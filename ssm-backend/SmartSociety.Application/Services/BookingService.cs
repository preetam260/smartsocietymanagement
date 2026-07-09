using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Application.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public BookingService(IUnitOfWork uow, INotificationService notificationService)
    {
        _uow = uow;
        _notificationService = notificationService;
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
                Date = dto.Date,
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
        var booking = await _uow.Bookings.GetByIdAsync(bookingId)
            ?? throw new NotFoundException("Booking", bookingId);

        if (booking.UserId != userId)
            throw new UnauthorizedException("You are not authorized to pay for this booking.");

        if (booking.Status != BookingStatus.Held)
            throw new BadRequestException("Only held bookings can be paid for.");

        if (booking.HoldExpiresAt != null && booking.HoldExpiresAt < DateTime.UtcNow)
        {
            booking.Status = BookingStatus.Expired;
            booking.UpdatedAt = DateTime.UtcNow;
            await _uow.Bookings.UpdateAsync(booking);
            await _uow.SaveChangesAsync();
            throw new BadRequestException("This booking hold has expired. Please create a new booking.");
        }

        var dummyOrderId = $"booking_order_{Guid.NewGuid():N}";

        booking.TransactionRef = dummyOrderId;
        booking.UpdatedAt = DateTime.UtcNow;

        await _uow.Bookings.UpdateAsync(booking);
        await _uow.SaveChangesAsync();

        return new CreatePaymentOrderResponseDto
        {
            OrderId = dummyOrderId,
            Amount = booking.TotalCost,
            Currency = "INR"
        };
    }

    public async Task<BookingResponseDto> VerifyPaymentAsync(VerifyBookingPaymentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Signature))
            throw new BadRequestException("Payment verification failed. Signature is required.");

        var booking = await _uow.Bookings.GetByIdAsync(dto.BookingId)
            ?? throw new NotFoundException("Booking", dto.BookingId);

        if (booking.Status != BookingStatus.Held)
            throw new BadRequestException("Only held bookings can be confirmed via payment.");

        if (booking.HoldExpiresAt != null && booking.HoldExpiresAt < DateTime.UtcNow)
        {
            booking.Status = BookingStatus.Expired;
            booking.UpdatedAt = DateTime.UtcNow;
            await _uow.Bookings.UpdateAsync(booking);
            await _uow.SaveChangesAsync();
            throw new BadRequestException("This booking hold has expired. Please create a new booking.");
        }

        booking.Status = BookingStatus.Confirmed;
        booking.TransactionRef = dto.PaymentId;
        booking.UpdatedAt = DateTime.UtcNow;

        await _uow.Bookings.UpdateAsync(booking);
        await _uow.SaveChangesAsync();

        var facility = await _uow.Facilities.GetByIdAsync(booking.FacilityId);
        await _notificationService.CreateAsync(
            booking.UserId,
            "Booking Confirmed",
            $"{facility?.Name ?? "Facility"} booked on {booking.Date:dd MMM yyyy} from {booking.StartTime:hh:mm tt} to {booking.EndTime:hh:mm tt}. Total: ₹{booking.TotalCost}. Ref: {booking.TransactionRef}.");

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
        var expiredHolds = await _uow.Bookings.GetExpiredHoldsAsync();
        foreach (var booking in expiredHolds)
        {
            booking.Status = BookingStatus.Expired;
            booking.UpdatedAt = DateTime.UtcNow;
            await _uow.Bookings.UpdateAsync(booking);

            await _notificationService.CreateAsync(
                booking.UserId,
                "Booking Expired",
                $"Your booking hold (ID: {booking.Id}) has expired because payment was not completed within 10 minutes.");
        }
        await _uow.SaveChangesAsync();
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