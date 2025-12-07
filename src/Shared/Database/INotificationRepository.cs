using Shared.Models;
using Shared.DTOs;

namespace Shared.Database;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<Notification?> GetByIdWithAttemptsAsync(Guid id);
    Task UpdateStatusAsync(Guid id, string status, string? error = null);
    Task IncrementRetriesAsync(Guid id);
    Task AddAttemptAsync(Guid notificationId, NotificationAttempt attempt);
}

