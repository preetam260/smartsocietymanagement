using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;

namespace SmartSociety.Repository.Interfaces;

public interface IVisitorRepository : IRepository<Visitor>
{
    // Called by the security staff 
    Task<Visitor?> GetByQrTokenAsync(string qrToken);
    // Get all visitors that are expected at an apartment
    Task<IEnumerable<Visitor>> GetByApartmentIdAsync(Guid id);
    // Get all visitors by a specific status
    Task<IEnumerable<Visitor>> GetByStatusAsync(VisitorStatus status);
}