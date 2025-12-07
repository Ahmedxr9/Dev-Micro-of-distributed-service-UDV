using System;
using System.Collections.Generic;

namespace Shared.DTOs;

public class NotificationStatusDto
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Retries { get; set; }
    public string? Errors { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<AttemptDto> Attempts { get; set; } = new();
}

public class AttemptDto
{
    public DateTime AttemptedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int RetryNumber { get; set; }
}

