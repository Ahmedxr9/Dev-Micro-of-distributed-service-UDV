using Microsoft.Extensions.Logging;

namespace EmailService.Providers;

public class MockEmailProvider : IEmailProvider
{
    private readonly ILogger<MockEmailProvider> _logger;
    private readonly Random _random = new();

    public MockEmailProvider(ILogger<MockEmailProvider> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string recipient, string message, Dictionary<string, object>? metadata = null)
    {
        _logger.LogInformation("MockEmailProvider: Sending email to {Recipient}", recipient);

        // Simulate network delay
        await Task.Delay(TimeSpan.FromMilliseconds(100 + _random.Next(200)));

        // Simulate occasional failures (10% failure rate)
        if (_random.Next(100) < 10)
        {
            _logger.LogWarning("MockEmailProvider: Simulated failure for {Recipient}", recipient);
            throw new Exception("Simulated SMTP server error");
        }

        _logger.LogInformation("MockEmailProvider: Email sent successfully to {Recipient}", recipient);
    }
}

