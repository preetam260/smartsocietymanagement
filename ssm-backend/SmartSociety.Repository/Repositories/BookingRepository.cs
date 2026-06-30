using SmartSociety.Domain.Models;
using SmartSociety.Domain.Enums;
using SmartSociety.Repository.Context;
using SmartSociety.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SmartSociety.Repository.Repositories;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(SmartSocietyDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<Booking>> GetAllBookingsByFacilityAsync(Guid facilityId){
        return await _context.Bookings
                       .Where(b => b.FacilityId == facilityId)
                       .OrderByDescending(b => b.Date)
                       .ToListAsync();
    }
    
    public async Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId)
    
        => await _context.Bookings
                .Where(b => b.UserId == userId)
                .ToListAsync();
    
    public async Task<IEnumerable<Booking>> GetByDateRangeAsync(Guid facilityId, DateTime start, DateTime end)
    {
        return await _context.Bookings
                       .Where(b => b.FacilityId == facilityId
                              && (b.Status == BookingStatus.Pending
                              || b.Status == BookingStatus.Confirmed
                              || (b.Status == BookingStatus.Held 
                                  && b.HoldExpiresAt != null 
                                  && b.HoldExpiresAt > DateTime.UtcNow))
                              && b.StartTime < end
                              && b.EndTime > start)
                       .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetExpiredHoldsAsync()
    {
        return await _context.Bookings
                       .Where(b => b.Status == BookingStatus.Held
                              && b.HoldExpiresAt != null
                              && b.HoldExpiresAt <= DateTime.UtcNow)
                       .ToListAsync();
    }
    
}