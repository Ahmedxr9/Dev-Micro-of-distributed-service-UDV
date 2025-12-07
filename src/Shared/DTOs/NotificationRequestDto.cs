using System.Collections.Generic;

namespace Shared.DTOs;

public class NotificationRequestDto
{
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

