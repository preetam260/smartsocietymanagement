using SmartSociety.Domain.Models;

namespace SmartSociety.Repository.Interfaces;

public interface IBillRepository : IRepository<Bill>
{
    Task<IEnumerable<Bill>> GetPendingBillsAsync();
    Task<IEnumerable<Bill>> GetByApartmentIdAsync(Guid id);
    Task<IEnumerable<Bill>> GetByPeriodAsync(string period);
    Task<Bill?> GetByApartmentAndPeriodAsync(Guid apartmentId, string period);
}
