using SmartSociety.Repository.Context;
using SmartSociety.Repository.Interfaces;
using SmartSociety.Domain.Models;
using SmartSociety.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SmartSociety.Repository.Repositories;

public class VisitorEntryRepository : Repository<VisitorEntry>, IVisitorEntryRepository
{
    public VisitorEntryRepository(SmartSocietyDbContext context) : base(context) {}

    public async Task<IEnumerable<VisitorEntry>> GetByVisitorIdAsync(Guid id)
    
        => await _context.VisitorEntries
            .Where(e => e.VisitorId == id)
            .OrderBy(e => e.CheckinTime)
            .ToListAsync();
    

    public async Task<VisitorEntry?> GetActiveEntryAsync(Guid visitorId)
    
        => await _context.VisitorEntries
            .FirstOrDefaultAsync(e => e.VisitorId == visitorId
                            && e.CheckoutTime == null);
    
}