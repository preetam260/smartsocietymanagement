using SmartSociety.Domain.Models;
using SmartSociety.Repository.Repositories;

namespace SmartSociety.Repository.Interfaces;

public interface IVisitorEntryRepository: IRepository<VisitorEntry>
{
    Task<IEnumerable<VisitorEntry>> GetByVisitorIdAsync(Guid id);
    Task<VisitorEntry?> GetActiveEntryAsync(Guid visitorId);

}