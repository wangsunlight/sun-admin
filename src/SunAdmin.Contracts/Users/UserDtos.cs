using SunAdmin.Domain.Enums;

namespace SunAdmin.Contracts.Users;

public sealed record UserDto(
    long Id,
    string UserName,
    string DisplayName,
    string Email,
    long? DepartmentId,
    string? DepartmentName,
    long? PositionId,
    string? PositionName,
    RecordStatus Status,
    bool IsBuiltIn,
    bool MustChangePassword,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles);

public sealed record UserQuery(
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null,
    RecordStatus? Status = null,
    long? RoleId = null,
    long? DepartmentId = null,
    long? PositionId = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null);

public sealed record CreateUserRequest(
    string UserName,
    string DisplayName,
    string Email,
    long? DepartmentId,
    long? PositionId,
    string Password,
    IReadOnlyList<long>? RoleIds);

public sealed record UpdateUserRequest(
    string DisplayName,
    string Email,
    long? DepartmentId,
    long? PositionId,
    RecordStatus Status);

public sealed record ResetPasswordRequest(string NewPassword);

public sealed record AssignUserRolesRequest(IReadOnlyList<long> RoleIds);

public sealed record BatchUserRequest(IReadOnlyList<long> UserIds);
