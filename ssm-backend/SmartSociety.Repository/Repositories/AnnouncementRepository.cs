using SmartSociety.Domain.Models;
using SmartSociety.Domain.Enums;
using SmartSociety.Repository.Context;
using SmartSociety.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SmartSociety.Repository.Repositories;

public class AnnouncementRepository : Repository<Announcement>, IAnnouncementRepository
{
    public AnnouncementRepository(SmartSocietyDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<Announcement>> GetActiveByAudienceAsync(UserRole role)
        => await _context.Announcements.Where(a => a.Audience == role && a.ExpiresAt > DateTime.UtcNow)
                                     .OrderByDescending(a => a.CreatedAt)
                                     .ToListAsync();
    public async Task<IEnumerable<Announcement>> GetPinnedAsync()
        => await _context.Announcements.Where(a => a.IsPinned && a.ExpiresAt > DateTime.UtcNow)
                                     .OrderByDescending(a => a.CreatedAt)
                                     .ToListAsync();
    

}