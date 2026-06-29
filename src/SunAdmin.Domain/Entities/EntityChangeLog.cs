using SunAdmin.Domain.Common;

namespace SunAdmin.Domain.Entities;

public sealed class EntityChangeLog : AuditableEntity
{
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public long? ChangedBy { get; set; }
    public string? ChangedByName { get; set; }
    public string? ChangedFields { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
