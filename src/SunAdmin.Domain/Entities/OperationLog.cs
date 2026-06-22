using SunAdmin.Domain.Common;

namespace SunAdmin.Domain.Entities;

public sealed class OperationLog : AuditableEntity
{
    public long? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Succeeded { get; set; }
    public long DurationMs { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? ErrorMessage { get; set; }
}
