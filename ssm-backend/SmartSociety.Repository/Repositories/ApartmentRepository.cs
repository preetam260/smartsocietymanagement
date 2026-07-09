using Microsoft.EntityFrameworkCore;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Context;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Repository.Repositories;

public class ApartmentRepository: Repository<Apartment>, IApartmentRepository
{
    public ApartmentRepository(SmartSocietyDbContext context) : base(context)
    {
        
    }
    public async Task<Apartment?> GetByBlockAndNumberAsync(string block, string number)
        => await _context.Apartments.FirstOrDefaultAsync(a => a.Block == block && a.Number == number);

    public async Task<IEnumerable<Apartment>> GetByOwnerIdAsync(Guid ownerId)
        => await _context.Apartments.Where(a => a.OwnerId == ownerId).ToListAsync();
}