using Microsoft.EntityFrameworkCore;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Context;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Repository.Repositories;

public class ComplaintRepository : Repository<Complaint>, IComplaintRepository
{
    public ComplaintRepository(SmartSocietyDbContext context) : base(context) {}

    public new async Task<IEnumerable<Complaint>> GetAllAsync()
    {
        return await _context.Complaints
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Complaint>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Complaints
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}
