using SunAdmin.Domain.Common;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Domain.Entities;

public sealed class ExportTask : AuditableEntity
{
    public string TaskName { get; set; } = string.Empty;
    public string ExportType { get; set; } = string.Empty;
    public ExportTaskStatus Status { get; set; } = ExportTaskStatus.Pending;
    public string? ParametersJson { get; set; }
    public string? FilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public long CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
