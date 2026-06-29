using SunAdmin.Contracts.Common;

namespace SunAdmin.Contracts.Audit;

public sealed record EntityChangeLogQuery(
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null,
    string? EntityName = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null);

public sealed record EntityChangeLogDto(
    long Id,
    string EntityName,
    string EntityId,
    string ChangeType,
    long? ChangedBy,
    string? ChangedByName,
    string? ChangedFields,
    string? BeforeJson,
    string? AfterJson,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt);
