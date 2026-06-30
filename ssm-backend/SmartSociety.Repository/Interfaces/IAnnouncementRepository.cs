using SmartSociety.Domain.Models;
using SmartSociety.Domain.Enums;

namespace SmartSociety.Repository.Interfaces;

public interface IAnnouncementRepository: IRepository<Announcement>
{
    Task<IEnumerable<Announcement>> GetActiveByAudienceAsync(UserRole role);
    Task<IEnumerable<Announcement>> GetPinnedAsync();
}