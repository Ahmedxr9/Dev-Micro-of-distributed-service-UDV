using Shared.Messaging;
using PushService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PushService.Services;

public class PushWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PushWorker> _logger;
    private IMessageConsumer? _consumer;

    public PushWorker(IServiceProvider serviceProvider, ILogger<PushWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Push Worker starting...");

        using var scope = _serviceProvider.CreateScope();
        _consumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();

        await _consumer.StartConsumingAsync(Shared.Messaging.QueueNames.Push, async message =>
        {
            _logger.LogInformation("Processing push notification: {NotificationId}", message.NotificationId);
            
            // Create a new scope for each message to ensure scoped services are properly resolved
            using var messageScope = _serviceProvider.CreateScope();
            var pushService = messageScope.ServiceProvider.GetRequiredService<IPushService>();
            await pushService.ProcessNotificationAsync(message);
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("Push Worker stopping...");
        if (_consumer != null)
        {
            await _consumer.StopAsync();
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}

