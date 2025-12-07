using Shared.Messaging;

namespace PushService.Services;

public interface IPushService
{
    Task ProcessNotificationAsync(NotificationMessage message);
}

