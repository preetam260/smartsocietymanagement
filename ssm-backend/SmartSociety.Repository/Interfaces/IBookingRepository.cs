using SmartSociety.Domain.Models;

namespace SmartSociety.Repository.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetAllBookingsByFacilityAsync(Guid facilityId);
    Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Booking>> GetByDateRangeAsync(Guid facilityId, DateTime start, DateTime end);
    Task<IEnumerable<Booking>> GetExpiredHoldsAsync();
}