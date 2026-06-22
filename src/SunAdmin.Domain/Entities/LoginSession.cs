using SunAdmin.Domain.Common;

namespace SunAdmin.Domain.Entities;

public sealed class LoginSession : AuditableEntity
{
    public string SessionId { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
