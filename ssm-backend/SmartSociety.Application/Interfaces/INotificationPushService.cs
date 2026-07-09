using SmartSociety.Application.DTOs;

namespace SmartSociety.Application.Interfaces;

public interface INotificationPushService
{
    Task PushAsync(Guid userId, NotificationResponseDto notification);
}
