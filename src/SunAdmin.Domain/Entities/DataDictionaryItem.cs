using SunAdmin.Domain.Common;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Domain.Entities;

public sealed class DataDictionaryItem : AuditableEntity
{
    public long DictionaryId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Enabled;
    public bool IsBuiltIn { get; set; }
}
