using SmartSociety.Domain.Models;
using SmartSociety.Repository.Repositories;

namespace SmartSociety.Repository.Interfaces;

public interface IVisitorEntryRepository: IRepository<VisitorEntry>
{
    // Get all entry records for a given visitor
    Task<IEnumerable<VisitorEntry>> GetByVisitorIdAsync(Guid id);
    // Gets the record where a visitor is currently checkedin
    Task<VisitorEntry?> GetActiveEntryAsync(Guid visitorId);

}