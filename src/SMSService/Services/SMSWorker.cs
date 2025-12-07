using Shared.Messaging;
using SMSService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SMSService.Services;

public class SMSWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SMSWorker> _logger;
    private IMessageConsumer? _consumer;

    public SMSWorker(IServiceProvider serviceProvider, ILogger<SMSWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SMS Worker starting...");

        using var scope = _serviceProvider.CreateScope();
        _consumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();

        await _consumer.StartConsumingAsync(Shared.Messaging.QueueNames.Sms, async message =>
        {
            _logger.LogInformation("Processing SMS notification: {NotificationId}", message.NotificationId);
            
            // Create a new scope for each message to ensure scoped services are properly resolved
            using var messageScope = _serviceProvider.CreateScope();
            var smsService = messageScope.ServiceProvider.GetRequiredService<ISMSService>();
            await smsService.ProcessNotificationAsync(message);
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("SMS Worker stopping...");
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

