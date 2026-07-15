using SmartSociety.Application.DTOs;

namespace SmartSociety.Application.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<BookingResponseDto>> GetAllAsync();        
    Task<PagedResult<BookingResponseDto>> GetAllPagedAsync(PaginationQuery query); 
    Task<BookingResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<BookingResponseDto>> GetByFacilityIdAsync(Guid facilityId);
    Task<PagedResult<BookingResponseDto>> GetByFacilityIdPagedAsync(Guid facilityId, PaginationQuery query);
    Task<IEnumerable<BookingResponseDto>> GetMyBookingsAsync(Guid userId);
    Task<IEnumerable<BookingResponseDto>> GetByUserIdAsync(Guid userId);
    Task<BookingResponseDto> CreateAsync(CreateBookingDto dto, Guid userId);
    Task<CreatePaymentOrderResponseDto> CreatePaymentOrderAsync(Guid bookingId, Guid userId);
    Task<BookingResponseDto> CompleteSimulatedPaymentAsync(Guid bookingId, CompleteBookingPaymentDto dto, Guid userId);
    Task CancelAsync(Guid id, Guid userId);
    Task CompleteAsync(Guid id);            
    Task ExpireHoldsAsync();            
}