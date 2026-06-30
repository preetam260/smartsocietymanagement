using SmartSociety.Domain.Models;
using SmartSociety.Repository.Context;
using SmartSociety.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SmartSociety.Repository.Repositories;

public class FacilityRepository : Repository<Facility>, IFacilityRepository
{
    public FacilityRepository(SmartSocietyDbContext context) : base(context) {}

    public async Task<IEnumerable<Facility>> GetActiveFacilitiesAsync()
    {
        return await _context.Facilities.Where(f => f.IsActive == true).ToListAsync();
    }

}