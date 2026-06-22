namespace SunAdmin.Contracts.Logs;

public sealed record LogQuery(
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null,
    bool? Succeeded = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null);

public sealed record OperationLogDto(
    long Id,
    long? UserId,
    string UserName,
    string Method,
    string Path,
    int StatusCode,
    bool Succeeded,
    long DurationMs,
    string? IpAddress,
    string? UserAgent,
    string? ErrorMessage,
    DateTime CreatedAt);

public sealed record LoginLogDto(
    long Id,
    long? UserId,
    string Account,
    string? UserName,
    bool Succeeded,
    string Message,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt);
