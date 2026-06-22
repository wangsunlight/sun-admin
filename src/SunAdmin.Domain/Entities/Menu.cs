using SunAdmin.Domain.Common;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Domain.Entities;

public sealed class Menu : AuditableEntity
{
    public long? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public MenuType Type { get; set; }
    public string? RoutePath { get; set; }
    public string? Component { get; set; }
    public string? Icon { get; set; }
    public string? PermissionCode { get; set; }
    public int SortOrder { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Enabled;
    public bool IsBuiltIn { get; set; }
}
