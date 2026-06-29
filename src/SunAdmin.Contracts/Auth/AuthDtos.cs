using SunAdmin.Contracts.Menus;

namespace SunAdmin.Contracts.Auth;

public sealed record LoginRequest(string Account, string Password);

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    CurrentUserDto User);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record ChangePasswordRequest(string OldPassword, string NewPassword);

public sealed record UpdateProfileRequest(
    string DisplayName,
    string Email,
    long? DepartmentId,
    long? PositionId);

public sealed record CurrentUserDto(
    long Id,
    string UserName,
    string DisplayName,
    string Email,
    long? DepartmentId,
    string? DepartmentName,
    long? PositionId,
    string? PositionName,
    bool MustChangePassword,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<MenuTreeNodeDto> Menus);
