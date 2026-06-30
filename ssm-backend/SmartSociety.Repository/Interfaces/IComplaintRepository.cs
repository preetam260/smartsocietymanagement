using SmartSociety.Domain.Models;

namespace SmartSociety.Repository.Interfaces;

public interface IComplaintRepository : IRepository<Complaint>
{
    Task<IEnumerable<Complaint>> GetByUserIdAsync(Guid userId);
    new Task<IEnumerable<Complaint>> GetAllAsync();
}
