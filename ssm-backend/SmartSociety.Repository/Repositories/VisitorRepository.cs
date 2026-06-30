using SmartSociety.Repository.Context;
using SmartSociety.Repository.Interfaces;
using SmartSociety.Domain.Models;
using SmartSociety.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SmartSociety.Repository.Repositories;

public class VisitorRepository : Repository<Visitor>, IVisitorRepository
{
    public VisitorRepository(SmartSocietyDbContext context) : base(context) {}

    public async Task<Visitor?> GetByQrTokenAsync(string qrToken)
    {
        return await _context.Visitors.FirstOrDefaultAsync(v => v.QrToken == qrToken);
    }

    public async Task<IEnumerable<Visitor>> GetByApartmentIdAsync(Guid id)
    {
        return await _context.Visitors.Where(v => v.ApartmentId == id)
                             .OrderByDescending(v => v.ETA)
                             .ToListAsync();
    }

    public async Task<IEnumerable<Visitor>> GetByStatusAsync(VisitorStatus status)
    {
        return await _context.Visitors
            .Where(v => v.Status == status)
            .OrderBy(v => v.ETA)
            .ToListAsync();
    }
}