using Shared.Database;
using Shared.Messaging;
using Shared.Models;
using SMSService.Providers;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Prometheus;

namespace SMSService.Services;

public class SMSService : ISMSService
{
    private readonly INotificationRepository _repository;
    private readonly ISMSProvider _smsProvider;
    private readonly ILogger<SMSService> _logger;
    private static readonly Counter ProcessedNotifications = Metrics
        .CreateCounter("sms_notifications_processed_total", "Total number of SMS notifications processed");
    private static readonly Counter FailedNotifications = Metrics
        .CreateCounter("sms_notifications_failed_total", "Total number of failed SMS notifications");
    private static readonly Histogram ProcessingDuration = Metrics
        .CreateHistogram("sms_notification_processing_duration_seconds", "Time spent processing SMS notifications");

    private readonly AsyncRetryPolicy _retryPolicy;

    public SMSService(
        INotificationRepository repository,
        ISMSProvider smsProvider,
        ILogger<SMSService> logger)
    {
        _repository = repository;
        _smsProvider = smsProvider;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, 
                        "Retry {RetryCount} for notification {NotificationId} after {Delay}s",
                        retryCount, context.ContainsKey("NotificationId") ? context["NotificationId"] : "unknown", timeSpan.TotalSeconds);
                });
    }

    public async Task ProcessNotificationAsync(NotificationMessage message)
    {
        using var timer = ProcessingDuration.NewTimer();
        using var activity = new ActivitySource("SMSService").StartActivity("ProcessSMSNotification");
        activity?.SetTag("notificationId", message.NotificationId.ToString());
        activity?.SetTag("recipient", message.Recipient);

        try
        {
            var context = new Context { { "NotificationId", message.NotificationId } };

            await _retryPolicy.ExecuteAsync(async (ctx) =>
            {
                await ProcessWithRetryAsync(message, ctx);
            }, context);

            ProcessedNotifications.Inc();
            _logger.LogInformation("Successfully processed SMS notification: {NotificationId}", message.NotificationId);
        }
        catch (Exception ex)
        {
            FailedNotifications.Inc();
            _logger.LogError(ex, "Failed to process SMS notification after retries: {NotificationId}", message.NotificationId);
            
            await _repository.UpdateStatusAsync(message.NotificationId, "Failed", ex.Message);
            
            await _repository.AddAttemptAsync(message.NotificationId, new NotificationAttempt
            {
                Id = Guid.NewGuid(),
                AttemptedAt = DateTime.UtcNow,
                Status = "Failed",
                ErrorMessage = ex.Message,
                RetryNumber = message.RetryCount
            });

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }
    }

    private async Task ProcessWithRetryAsync(NotificationMessage message, Context context)
    {
        var attemptNumber = message.RetryCount + 1;
        
        // Update status to processing
        await _repository.UpdateStatusAsync(message.NotificationId, "Processing");

        // Add attempt record
        var attempt = new NotificationAttempt
        {
            Id = Guid.NewGuid(),
            AttemptedAt = DateTime.UtcNow,
            Status = "Processing",
            RetryNumber = attemptNumber
        };
        await _repository.AddAttemptAsync(message.NotificationId, attempt);

        try
        {
            // Call SMS provider
            await _smsProvider.SendSMSAsync(message.Recipient, message.Message, message.Metadata);

            // Update status to success
            await _repository.UpdateStatusAsync(message.NotificationId, "Sent");
            
            attempt.Status = "Sent";
            attempt.AttemptedAt = DateTime.UtcNow;
            await _repository.AddAttemptAsync(message.NotificationId, attempt);

            _logger.LogInformation("SMS sent successfully to {Recipient} for notification {NotificationId}", 
                message.Recipient, message.NotificationId);
        }
        catch (Exception ex)
        {
            await _repository.IncrementRetriesAsync(message.NotificationId);
            
            attempt.Status = "Failed";
            attempt.ErrorMessage = ex.Message;
            await _repository.AddAttemptAsync(message.NotificationId, attempt);

            _logger.LogWarning(ex, "SMS send attempt {AttemptNumber} failed for notification {NotificationId}", 
                attemptNumber, message.NotificationId);

            throw; // Re-throw to trigger retry policy
        }
    }
}

