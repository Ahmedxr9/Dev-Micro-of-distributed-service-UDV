using Microsoft.Extensions.Logging;

namespace PushService.Providers;

public class MockPushProvider : IPushProvider
{
    private readonly ILogger<MockPushProvider> _logger;
    private readonly Random _random = new();

    public MockPushProvider(ILogger<MockPushProvider> logger)
    {
        _logger = logger;
    }

    public async Task SendPushAsync(string recipient, string message, Dictionary<string, object>? metadata = null)
    {
        _logger.LogInformation("MockPushProvider: Sending push notification to {Recipient}", recipient);

        // Simulate network delay
        await Task.Delay(TimeSpan.FromMilliseconds(80 + _random.Next(150)));

        // Simulate occasional failures (10% failure rate)
        if (_random.Next(100) < 10)
        {
            _logger.LogWarning("MockPushProvider: Simulated failure for {Recipient}", recipient);
            throw new Exception("Simulated FCM API error");
        }

        _logger.LogInformation("MockPushProvider: Push notification sent successfully to {Recipient}", recipient);
    }
}

