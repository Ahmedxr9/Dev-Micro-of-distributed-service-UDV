namespace EmailService.Providers;

public interface IEmailProvider
{
    Task SendEmailAsync(string recipient, string message, Dictionary<string, object>? metadata = null);
}

