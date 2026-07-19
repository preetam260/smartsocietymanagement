using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;

namespace SmartSociety.Repository.Interfaces;

public interface IVisitorRepository : IRepository<Visitor>
{
    Task<Visitor?> GetByQrTokenAsync(string qrToken);
    Task<IEnumerable<Visitor>> GetByApartmentIdAsync(Guid id);
    Task<IEnumerable<Visitor>> GetByStatusAsync(VisitorStatus status);
    Task<IEnumerable<Visitor>> GetByEmailAsync(string email, Guid apartmentId);
}