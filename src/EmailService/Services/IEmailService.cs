using Shared.Messaging;

namespace EmailService.Services;

public interface IEmailService
{
    Task ProcessNotificationAsync(NotificationMessage message);
}

