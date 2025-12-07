using System;
using System.Collections.Generic;

namespace Shared.Models;

public class Notification
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public int Retries { get; set; }
    public string? Errors { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public List<NotificationAttempt> Attempts { get; set; } = new();
}

public class NotificationAttempt
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;
    public DateTime AttemptedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int RetryNumber { get; set; }
}

