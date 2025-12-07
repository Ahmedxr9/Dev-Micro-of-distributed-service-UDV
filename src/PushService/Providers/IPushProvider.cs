namespace PushService.Providers;

public interface IPushProvider
{
    Task SendPushAsync(string recipient, string message, Dictionary<string, object>? metadata = null);
}

