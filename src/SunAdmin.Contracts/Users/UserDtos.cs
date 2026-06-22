using SunAdmin.Domain.Enums;

namespace SunAdmin.Contracts.Users;

public sealed record UserDto(
    long Id,
    string UserName,
    string DisplayName,
    string Email,
    RecordStatus Status,
    bool IsBuiltIn,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles);

public sealed record UserQuery(int PageIndex = 1, int PageSize = 20, string? Keyword = null);

public sealed record CreateUserRequest(
    string UserName,
    string DisplayName,
    string Email,
    string Password,
    IReadOnlyList<long>? RoleIds);

public sealed record UpdateUserRequest(
    string DisplayName,
    string Email,
    RecordStatus Status);

public sealed record ResetPasswordRequest(string NewPassword);

public sealed record AssignUserRolesRequest(IReadOnlyList<long> RoleIds);
