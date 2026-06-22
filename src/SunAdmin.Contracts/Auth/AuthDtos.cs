using SunAdmin.Contracts.Menus;

namespace SunAdmin.Contracts.Auth;

public sealed record LoginRequest(string Account, string Password);

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAt,
    CurrentUserDto User);

public sealed record ChangePasswordRequest(string OldPassword, string NewPassword);

public sealed record CurrentUserDto(
    long Id,
    string UserName,
    string DisplayName,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<MenuTreeNodeDto> Menus);
