using SunAdmin.Domain.Enums;

namespace SunAdmin.Contracts.Roles;

public sealed record RoleDto(
    long Id,
    string Code,
    string Name,
    string? Description,
    RecordStatus Status,
    bool IsBuiltIn,
    DateTime CreatedAt,
    IReadOnlyList<long> MenuIds);

public sealed record RoleQuery(int PageIndex = 1, int PageSize = 20, string? Keyword = null);

public sealed record CreateRoleRequest(
    string Code,
    string Name,
    string? Description);

public sealed record UpdateRoleRequest(
    string Name,
    string? Description,
    RecordStatus Status);

public sealed record AssignRoleMenusRequest(IReadOnlyList<long> MenuIds);
