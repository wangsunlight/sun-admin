using SunAdmin.Domain.Enums;

namespace SunAdmin.Contracts.Positions;

public sealed record PositionDto(
    long Id,
    string Code,
    string Name,
    string? Description,
    int SortOrder,
    RecordStatus Status,
    bool IsBuiltIn,
    DateTime CreatedAt);

public sealed record PositionQuery(int PageIndex = 1, int PageSize = 20, string? Keyword = null);

public sealed record CreatePositionRequest(
    string Code,
    string Name,
    string? Description,
    int SortOrder);

public sealed record UpdatePositionRequest(
    string Code,
    string Name,
    string? Description,
    int SortOrder,
    RecordStatus Status);
