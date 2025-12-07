using Shared.Messaging;
using EmailService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Services;

public class EmailWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailWorker> _logger;
    private IMessageConsumer? _consumer;

    public EmailWorker(IServiceProvider serviceProvider, ILogger<EmailWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Worker starting...");

        using var scope = _serviceProvider.CreateScope();
        _consumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();

        await _consumer.StartConsumingAsync(Shared.Messaging.QueueNames.Email, async message =>
        {
            _logger.LogInformation("Processing email notification: {NotificationId}", message.NotificationId);
            
            // Create a new scope for each message to ensure scoped services are properly resolved
            using var messageScope = _serviceProvider.CreateScope();
            var emailService = messageScope.ServiceProvider.GetRequiredService<IEmailService>();
            await emailService.ProcessNotificationAsync(message);
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("Email Worker stopping...");
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

