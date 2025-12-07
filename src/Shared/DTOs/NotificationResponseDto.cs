using System;

namespace Shared.DTOs;

public class NotificationResponseDto
{
    public Guid NotificationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

