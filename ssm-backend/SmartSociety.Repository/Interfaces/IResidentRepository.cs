using SmartSociety.Domain.Models;

namespace SmartSociety.Repository.Interfaces;

public interface IResidentRepository : IRepository<Resident> {
    // Returns all resident records linked to an apartment
    Task<IEnumerable<Resident>> GetByApartmentIdAsync(Guid id);
    // Returns the active residency record for a user
    Task<Resident?> GetCurrentByUserIdAsync(Guid id);
    // Get all active residency records 
    Task<IEnumerable<Resident>> GetAllCurrentAsync();
}