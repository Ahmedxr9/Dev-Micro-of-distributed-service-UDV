using System;
using System.Collections.Generic;

namespace Shared.Messaging;

public class NotificationMessage
{
    public Guid NotificationId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

