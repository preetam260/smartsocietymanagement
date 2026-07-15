using SmartSociety.Domain.Models;
using SmartSociety.Repository.Context;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Repository.Repositories;

public class ResidentRepository: Repository<Resident>, IResidentRepository
{
    public ResidentRepository(SmartSocietyDbContext context): base(context)
    {
        
    }

    public async Task<IEnumerable<Resident>> GetByApartmentIdAsync(Guid id)
    {
        return await _context.Residents.Where(r => r.ApartmentId == id).ToListAsync();
    }
    public async Task<Resident?> GetCurrentByUserIdAsync(Guid userId)
    {
        return await _context.Residents.FirstOrDefaultAsync(
            r => r.UserId == userId && r.MoveOutDate == null && !r.IsDeleted);
    }
    public async Task<IEnumerable<Resident>> GetAllCurrentAsync()
    {
        return await _context.Residents
            .Where(r => r.MoveOutDate == null)
            .ToListAsync();
    }
}
