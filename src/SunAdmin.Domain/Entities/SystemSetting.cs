using SunAdmin.Domain.Common;

namespace SunAdmin.Domain.Entities;

public sealed class SystemSetting : AuditableEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
