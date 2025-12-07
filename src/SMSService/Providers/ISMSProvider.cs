namespace SMSService.Providers;

public interface ISMSProvider
{
    Task SendSMSAsync(string recipient, string message, Dictionary<string, object>? metadata = null);
}

