using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;

namespace SmartSociety.Repository.Interfaces;

public interface IUserRepository : IRepository<User>
{
    // Used during login - AuthService
    Task<User?> GetByEmailAsync(string email); 
    // Returns all active users by specified role
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role); 
}