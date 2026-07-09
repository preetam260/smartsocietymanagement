using SmartSociety.Application.DTOs;

namespace SmartSociety.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<BookingResponseDto>> GetByFacilityIdAsync(Guid facilityId);
    Task<PagedResult<BookingResponseDto>> GetByFacilityIdPagedAsync(Guid facilityId, PaginationQuery query);
    Task<IEnumerable<BookingResponseDto>> GetMyBookingsAsync(Guid userId);
    Task<IEnumerable<BookingResponseDto>> GetByUserIdAsync(Guid userId); // admin use
    Task<BookingResponseDto> CreateAsync(CreateBookingDto dto, Guid userId);
    Task<CreatePaymentOrderResponseDto> CreatePaymentOrderAsync(Guid bookingId, Guid userId);
    Task<BookingResponseDto> VerifyPaymentAsync(VerifyBookingPaymentDto dto);
    Task CancelAsync(Guid id, Guid userId); // userId for ownership check
    Task CompleteAsync(Guid id);            // admin or background job
    Task ExpireHoldsAsync();                // expire stale held bookings
}