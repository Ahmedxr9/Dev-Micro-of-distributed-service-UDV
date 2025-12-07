using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Models;
using Shared.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Database;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<NotificationRepository>? _logger;

    public NotificationRepository(NotificationDbContext context, ILogger<NotificationRepository>? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        _logger?.LogInformation("Created notification with ID: {NotificationId}", notification.Id);
        return notification;
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await _context.Notifications.FindAsync(id);
    }

    public async Task<Notification?> GetByIdWithAttemptsAsync(Guid id)
    {
        return await _context.Notifications
            .Include(n => n.Attempts)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task UpdateStatusAsync(Guid id, string status, string? error = null)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification != null)
        {
            notification.Status = status;
            notification.UpdatedAt = DateTime.UtcNow;
            if (error != null)
            {
                notification.Errors = string.IsNullOrEmpty(notification.Errors) 
                    ? error 
                    : $"{notification.Errors}; {error}";
            }
            await _context.SaveChangesAsync();
            _logger?.LogInformation("Updated notification {NotificationId} status to {Status}", id, status);
        }
    }

    public async Task IncrementRetriesAsync(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification != null)
        {
            notification.Retries++;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddAttemptAsync(Guid notificationId, NotificationAttempt attempt)
    {
        attempt.NotificationId = notificationId;
        _context.NotificationAttempts.Add(attempt);
        await _context.SaveChangesAsync();
        _logger?.LogInformation("Added attempt for notification {NotificationId}, Retry: {RetryNumber}", 
            notificationId, attempt.RetryNumber);
    }
}

