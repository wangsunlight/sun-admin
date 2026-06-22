using SunAdmin.Domain.Enums;

namespace SunAdmin.Contracts.Roles;

public sealed record RoleDto(
    long Id,
    string Code,
    string Name,
    string? Description,
    RoleDataScope DataScope,
    RecordStatus Status,
    bool IsBuiltIn,
    int UserCount,
    DateTime CreatedAt,
    IReadOnlyList<long> MenuIds);

public sealed record RoleQuery(int PageIndex = 1, int PageSize = 20, string? Keyword = null);

public sealed record CreateRoleRequest(
    string Code,
    string Name,
    string? Description,
    RoleDataScope DataScope = RoleDataScope.All);

public sealed record UpdateRoleRequest(
    string Name,
    string? Description,
    RoleDataScope DataScope,
    RecordStatus Status);

public sealed record AssignRoleMenusRequest(IReadOnlyList<long> MenuIds);
