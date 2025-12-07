namespace Shared.Messaging;

public static class QueueNames
{
    public const string Notifications = "notifications.queue";
    public const string Email = "email.queue";
    public const string Sms = "sms.queue";
    public const string Push = "push.queue";
    
    public const string DeadLetterExchange = "notifications.dlx";
}

