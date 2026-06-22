using SunAdmin.Domain.Common;

namespace SunAdmin.Domain.Entities;

public sealed class LoginLog : AuditableEntity
{
    public long? UserId { get; set; }
    public string Account { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
