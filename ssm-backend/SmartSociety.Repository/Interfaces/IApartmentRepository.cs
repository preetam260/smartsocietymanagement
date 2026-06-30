using SmartSociety.Domain.Models;

namespace SmartSociety.Repository.Interfaces;

public interface IApartmentRepository: IRepository<Apartment>
{
    Task<Apartment?> GetByBlockAndNumberAsync(string block, string number);
    Task<IEnumerable<Apartment>> GetByOwnerIdAsync(Guid ownerId);
}