using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;

    public NotificationService(IUnitOfWork uow, IEmailService emailService)
    {
        _uow = uow;
        _emailService = emailService;
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetByUserIdAsync(Guid userId)
    {
        var notifications = await _uow.Notifications.GetByUserIdAsync(userId);
        return notifications.Select(MapToDto);
    }

    public async Task<PagedResult<NotificationResponseDto>> GetByUserIdPagedAsync(Guid userId, PaginationQuery query)
    {
        var notifications = await _uow.Notifications.GetByUserIdAsync(userId);
        var dtos = notifications.Select(MapToDto);
        return PagedResult<NotificationResponseDto>.Create(dtos, query.PageNumber, query.PageSize, query.Search,
            [n => n.Title, n => n.Message]);
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetUnreadByUserIdAsync(Guid userId)
    {
        var notifications = await _uow.Notifications.GetUnreadByUserIdAsync(userId);
        return notifications.Select(MapToDto);
    }

    public async Task MarkAsReadAsync(Guid id, Guid userId)
    {
        var notification = await _uow.Notifications.GetByIdAsync(id)
            ?? throw new NotFoundException("Notification", id);

        if (notification.UserId != userId)
            throw new UnauthorizedException("You are not authorized to modify this notification.");

        notification.IsRead = true;
        notification.UpdatedAt = DateTime.UtcNow;

        await _uow.Notifications.UpdateAsync(notification);
        await _uow.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _uow.Notifications.GetUnreadByUserIdAsync(userId);
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.UpdatedAt = DateTime.UtcNow;
            await _uow.Notifications.UpdateAsync(n);
        }
        await _uow.SaveChangesAsync();
    }

    public async Task CreateAsync(Guid userId, string title, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message
        };

        await _uow.Notifications.AddAsync(notification);
        await _uow.SaveChangesAsync();

        var user = await _uow.Users.GetByIdAsync(userId);
        if (user != null && !string.IsNullOrEmpty(user.Email) && user.IsActive && !user.IsDeleted)
        {
            try
            {
                await _emailService.SendAsync(
                    user.Email,
                    $"SmartSociety — {title}",
                    $@"
                    <h3>{title}</h3>
                    <p>{message}</p>
                    <br/>
                    <small>This is an automated alert from SmartSociety.</small>"
                );
            }
            catch {}
        }
    }

    private static NotificationResponseDto MapToDto(Notification n) => new()
    {
        Id = n.Id,
        Title = n.Title,
        Message = n.Message,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };
}