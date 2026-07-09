using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;

namespace SmartSociety.Repository.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email); 
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role); 
}