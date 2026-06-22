using SunAdmin.Domain.Common;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Domain.Entities;

public sealed class Role : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RoleDataScope DataScope { get; set; } = RoleDataScope.All;
    public RecordStatus Status { get; set; } = RecordStatus.Enabled;
    public bool IsBuiltIn { get; set; }
}
