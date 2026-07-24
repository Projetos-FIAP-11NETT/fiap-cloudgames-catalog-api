using System;
using System.Collections.Generic;
using System.Text;

namespace FiapCloudGames.Catalog.Domain.Entities;

public class RequestLog
{
    public string CorrelationId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}
