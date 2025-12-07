using Shared.Messaging;

namespace SMSService.Services;

public interface ISMSService
{
    Task ProcessNotificationAsync(NotificationMessage message);
}

