using SmartSociety.Domain.Models;
using SmartSociety.Domain.Enums;
using SmartSociety.Repository.Interfaces;
using SmartSociety.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace SmartSociety.Repository.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(SmartSocietyDbContext context) : base(context)
    {
        
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
    {
        return await _context.Users.Where(x => x.Role == role && x.IsActive).ToListAsync();
    }
}