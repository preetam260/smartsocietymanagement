using SmartSociety.Application.DTOs;

namespace SmartSociety.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponseDto>> GetByUserIdAsync(Guid userId);
    Task<PagedResult<NotificationResponseDto>> GetByUserIdPagedAsync(Guid userId, PaginationQuery query);
    Task<IEnumerable<NotificationResponseDto>> GetUnreadByUserIdAsync(Guid userId);
    Task MarkAsReadAsync(Guid id, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task CreateAsync(Guid userId, string title, string message); 
}