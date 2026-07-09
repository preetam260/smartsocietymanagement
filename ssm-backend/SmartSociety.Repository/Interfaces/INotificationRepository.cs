using SmartSociety.Domain.Models;

namespace SmartSociety.Repository.Interfaces;

public interface INotificationRepository: IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);
}