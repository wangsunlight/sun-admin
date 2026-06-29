using SunAdmin.Domain.Common;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Domain.Entities;

public sealed class SystemNotification : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationLevel Level { get; set; } = NotificationLevel.Info;
    public DateTime? PublishAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsPinned { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Enabled;
}
