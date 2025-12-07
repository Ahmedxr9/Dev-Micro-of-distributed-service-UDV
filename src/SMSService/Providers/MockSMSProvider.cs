using Microsoft.Extensions.Logging;

namespace SMSService.Providers;

public class MockSMSProvider : ISMSProvider
{
    private readonly ILogger<MockSMSProvider> _logger;
    private readonly Random _random = new();

    public MockSMSProvider(ILogger<MockSMSProvider> logger)
    {
        _logger = logger;
    }

    public async Task SendSMSAsync(string recipient, string message, Dictionary<string, object>? metadata = null)
    {
        _logger.LogInformation("MockSMSProvider: Sending SMS to {Recipient}", recipient);

        // Simulate network delay
        await Task.Delay(TimeSpan.FromMilliseconds(150 + _random.Next(300)));

        // Simulate occasional failures (10% failure rate)
        if (_random.Next(100) < 10)
        {
            _logger.LogWarning("MockSMSProvider: Simulated failure for {Recipient}", recipient);
            throw new Exception("Simulated Twilio API error");
        }

        _logger.LogInformation("MockSMSProvider: SMS sent successfully to {Recipient}", recipient);
    }
}

