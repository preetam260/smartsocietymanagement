using SmartSociety.Domain.Models;
using SmartSociety.Repository.Context;
using SmartSociety.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SmartSociety.Repository.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(SmartSocietyDbContext context) : base(context){}

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Notifications.Where(n => n.UserId == userId)
                                    .OrderByDescending(n => n.CreatedAt)
                                    .ToListAsync();
    }
    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
    {
        return await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead)
                                    .OrderByDescending(n => n.CreatedAt)
                                    .ToListAsync();
    }


}