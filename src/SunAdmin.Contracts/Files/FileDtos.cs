namespace SunAdmin.Contracts.Files;

public sealed record FileQuery(
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null);

public sealed record FileResourceDto(
    long Id,
    string FileName,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string StorageProvider,
    string StoragePath,
    long? UploadedBy,
    DateTime CreatedAt);

public sealed record CreateFileResourceRequest(
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string StorageProvider,
    string StoragePath);
