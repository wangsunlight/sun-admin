using SunAdmin.Domain.Common;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Domain.Entities;

public sealed class CodeGenerationTemplate : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string TemplateKey { get; set; } = string.Empty;
    public string TargetKind { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public RecordStatus Status { get; set; } = RecordStatus.Enabled;
    public bool IsBuiltIn { get; set; }
}
