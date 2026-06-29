using SunAdmin.Domain.Common;

namespace SunAdmin.Domain.Entities;

public sealed class FileResource : AuditableEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string StorageProvider { get; set; } = "local";
    public string StoragePath { get; set; } = string.Empty;
    public long? UploadedBy { get; set; }
}
