using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Application.Menus;
using SunAdmin.Contracts.Auth;
using SunAdmin.Domain.Constants;
using SunAdmin.Domain.Entities;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Infrastructure.Services;

public sealed class AuthService(
    IFreeSql freeSql,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await freeSql.Select<User>()
            .Where(x => x.DeletedAt == null && (x.UserName == request.Account || x.Email == request.Account))
            .FirstAsync(cancellationToken);

        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new BusinessException("UNAUTHORIZED", "Invalid account or password.");
        }

        if (user.Status != RecordStatus.Enabled)
        {
            throw new BusinessException("FORBIDDEN", "User is disabled.");
        }

        var roles = await GetRoleCodesAsync(user.Id, cancellationToken);
        var token = jwtTokenService.CreateToken(user, roles);
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<User>().SetSource(user).ExecuteAffrowsAsync(cancellationToken);

        return new LoginResponse(token.AccessToken, token.ExpiresAt, await BuildCurrentUserAsync(user, roles, cancellationToken));
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId ?? throw new BusinessException("UNAUTHORIZED", "User is not authenticated.");
        var user = await freeSql.Select<User>().Where(x => x.Id == userId && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("UNAUTHORIZED", "User does not exist.");
        var roles = await GetRoleCodesAsync(user.Id, cancellationToken);
        return await BuildCurrentUserAsync(user, roles, cancellationToken);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId ?? throw new BusinessException("UNAUTHORIZED", "User is not authenticated.");
        var user = await freeSql.Select<User>().Where(x => x.Id == userId && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "User not found.");
        if (!passwordHasher.VerifyPassword(request.OldPassword, user.PasswordHash))
        {
            throw new BusinessException("BUSINESS_ERROR", "Old password is incorrect.");
        }

        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<User>().SetSource(user).ExecuteAffrowsAsync(cancellationToken);
    }

    private async Task<CurrentUserDto> BuildCurrentUserAsync(User user, IReadOnlyList<string> roles, CancellationToken cancellationToken)
    {
        var menus = await GetUserMenusAsync(user.Id, roles.Contains(SystemRoleCodes.SuperAdmin), cancellationToken);
        var permissions = menus
            .Where(x => !string.IsNullOrWhiteSpace(x.PermissionCode))
            .Select(x => x.PermissionCode!)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        return new CurrentUserDto(user.Id, user.UserName, user.DisplayName, user.Email, roles, permissions, MenuTreeBuilder.Build(menus));
    }

    private async Task<IReadOnlyList<string>> GetRoleCodesAsync(long userId, CancellationToken cancellationToken)
    {
        return await freeSql.Select<UserRole, Role>()
            .InnerJoin((userRole, role) => userRole.RoleId == role.Id)
            .Where((userRole, role) => userRole.UserId == userId && role.Status == RecordStatus.Enabled && role.DeletedAt == null)
            .ToListAsync((userRole, role) => role.Code, cancellationToken);
    }

    private async Task<IReadOnlyList<Menu>> GetUserMenusAsync(long userId, bool isSuperAdmin, CancellationToken cancellationToken)
    {
        if (isSuperAdmin)
        {
            return await freeSql.Select<Menu>()
                .Where(x => x.Status == RecordStatus.Enabled && x.DeletedAt == null)
                .OrderBy(x => x.SortOrder)
                .ToListAsync(cancellationToken);
        }

        return await freeSql.Select<UserRole, Role, RoleMenu, Menu>()
            .InnerJoin((userRole, role, roleMenu, menu) => userRole.RoleId == role.Id)
            .InnerJoin((userRole, role, roleMenu, menu) => role.Id == roleMenu.RoleId)
            .InnerJoin((userRole, role, roleMenu, menu) => roleMenu.MenuId == menu.Id)
            .Where((userRole, role, roleMenu, menu) =>
                userRole.UserId == userId &&
                role.Status == RecordStatus.Enabled &&
                role.DeletedAt == null &&
                menu.Status == RecordStatus.Enabled &&
                menu.DeletedAt == null)
            .ToListAsync((userRole, role, roleMenu, menu) => menu, cancellationToken);
    }
}
