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
    IRequestContext requestContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IPasswordPolicyService passwordPolicyService) : IAuthService
{
    private const int RefreshTokenDays = 7;

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await freeSql.Select<User>()
            .Where(x => x.DeletedAt == null && (x.UserName == request.Account || x.Email == request.Account))
            .FirstAsync(cancellationToken);

        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            await WriteLoginLogAsync(user, request.Account, false, "账号或密码错误。", cancellationToken);
            throw new BusinessException("UNAUTHORIZED", "Invalid account or password.");
        }

        if (user.Status != RecordStatus.Enabled)
        {
            await WriteLoginLogAsync(user, request.Account, false, "账号已禁用。", cancellationToken);
            throw new BusinessException("FORBIDDEN", "User is disabled.");
        }

        var roles = await GetRoleCodesAsync(user.Id, cancellationToken);
        var sessionId = Guid.NewGuid().ToString("N");
        var token = jwtTokenService.CreateToken(user, roles, sessionId);
        var refreshToken = CreateRefreshToken(sessionId);
        await freeSql.Insert(new LoginSession
        {
            SessionId = sessionId,
            UserId = user.Id,
            UserName = user.UserName,
            IpAddress = requestContext.IpAddress,
            UserAgent = requestContext.UserAgent,
            ExpiresAt = token.ExpiresAt,
            RefreshTokenHash = passwordHasher.HashPassword(refreshToken),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenDays),
            LastSeenAt = DateTime.UtcNow
        }).ExecuteAffrowsAsync(cancellationToken);

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<User>().SetSource(user).ExecuteAffrowsAsync(cancellationToken);
        await WriteLoginLogAsync(user, request.Account, true, "登录成功。", cancellationToken);

        return new LoginResponse(token.AccessToken, refreshToken, token.ExpiresAt, await BuildCurrentUserAsync(user, roles, cancellationToken));
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var sessionId = ResolveSessionId(request.RefreshToken);
        var session = await freeSql.Select<LoginSession>().Where(x => x.SessionId == sessionId).FirstAsync(cancellationToken)
            ?? throw new BusinessException("UNAUTHORIZED", "Refresh token is invalid.");
        if (session.RevokedAt is not null ||
            session.RefreshTokenExpiresAt is null ||
            session.RefreshTokenExpiresAt <= DateTime.UtcNow ||
            string.IsNullOrWhiteSpace(session.RefreshTokenHash) ||
            !passwordHasher.VerifyPassword(request.RefreshToken, session.RefreshTokenHash))
        {
            throw new BusinessException("UNAUTHORIZED", "Refresh token is invalid.");
        }

        var user = await freeSql.Select<User>().Where(x => x.Id == session.UserId && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("UNAUTHORIZED", "User does not exist.");
        if (user.Status != RecordStatus.Enabled)
        {
            throw new BusinessException("FORBIDDEN", "User is disabled.");
        }

        var roles = await GetRoleCodesAsync(user.Id, cancellationToken);
        var token = jwtTokenService.CreateToken(user, roles, session.SessionId);
        var refreshToken = CreateRefreshToken(session.SessionId);
        session.ExpiresAt = token.ExpiresAt;
        session.RefreshTokenHash = passwordHasher.HashPassword(refreshToken);
        session.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenDays);
        session.LastSeenAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<LoginSession>().SetSource(session).ExecuteAffrowsAsync(cancellationToken);
        return new LoginResponse(token.AccessToken, refreshToken, token.ExpiresAt, await BuildCurrentUserAsync(user, roles, cancellationToken));
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUser.SessionId))
        {
            return;
        }

        var session = await freeSql.Select<LoginSession>().Where(x => x.SessionId == currentUser.SessionId).FirstAsync(cancellationToken);
        if (session is null || session.RevokedAt is not null)
        {
            return;
        }

        session.RevokedAt = DateTime.UtcNow;
        session.RevokedReason = "logout";
        session.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<LoginSession>().SetSource(session).ExecuteAffrowsAsync(cancellationToken);
    }

    public async Task LogoutAllAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        if (!userId.HasValue)
        {
            return;
        }

        var sessions = await freeSql.Select<LoginSession>()
            .Where(x => x.UserId == userId.Value && x.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var session in sessions)
        {
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedReason = "logout_all";
            session.UpdatedAt = DateTime.UtcNow;
        }

        if (sessions.Count > 0)
        {
            await freeSql.Update<LoginSession>().SetSource(sessions).ExecuteAffrowsAsync(cancellationToken);
        }
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId ?? throw new BusinessException("UNAUTHORIZED", "User is not authenticated.");
        var user = await freeSql.Select<User>().Where(x => x.Id == userId && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("UNAUTHORIZED", "User does not exist.");
        var roles = await GetRoleCodesAsync(user.Id, cancellationToken);
        return await BuildCurrentUserAsync(user, roles, cancellationToken);
    }

    public async Task<CurrentUserDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId ?? throw new BusinessException("UNAUTHORIZED", "User is not authenticated.");
        var user = await freeSql.Select<User>().Where(x => x.Id == userId && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "User not found.");

        if (await freeSql.Select<User>().Where(x => x.DeletedAt == null && x.Id != userId && x.Email == request.Email).AnyAsync(cancellationToken))
        {
            throw new BusinessException("CONFLICT", "Email already exists.");
        }

        user.DisplayName = request.DisplayName;
        user.Email = request.Email;
        user.DepartmentId = request.DepartmentId;
        user.PositionId = request.PositionId;
        user.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<User>().SetSource(user).ExecuteAffrowsAsync(cancellationToken);
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

        passwordPolicyService.Validate(request.NewPassword);
        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        user.MustChangePassword = false;
        user.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<User>().SetSource(user).ExecuteAffrowsAsync(cancellationToken);
    }

    private async Task<CurrentUserDto> BuildCurrentUserAsync(User user, IReadOnlyList<string> roles, CancellationToken cancellationToken)
    {
        var department = user.DepartmentId.HasValue
            ? await freeSql.Select<Department>().Where(x => x.Id == user.DepartmentId.Value && x.DeletedAt == null).FirstAsync(cancellationToken)
            : null;
        var position = user.PositionId.HasValue
            ? await freeSql.Select<Position>().Where(x => x.Id == user.PositionId.Value && x.DeletedAt == null).FirstAsync(cancellationToken)
            : null;
        var menus = await GetUserMenusAsync(user.Id, roles.Contains(SystemRoleCodes.SuperAdmin), cancellationToken);
        var permissions = menus
            .Where(x => !string.IsNullOrWhiteSpace(x.PermissionCode))
            .Select(x => x.PermissionCode!)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        return new CurrentUserDto(
            user.Id,
            user.UserName,
            user.DisplayName,
            user.Email,
            user.DepartmentId,
            department?.Name,
            user.PositionId,
            position?.Name,
            user.MustChangePassword,
            roles,
            permissions,
            MenuTreeBuilder.Build(menus));
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

    private async Task WriteLoginLogAsync(User? user, string account, bool succeeded, string message, CancellationToken cancellationToken)
    {
        await freeSql.Insert(new LoginLog
        {
            UserId = user?.Id,
            Account = account,
            UserName = user?.UserName,
            Succeeded = succeeded,
            Message = message,
            IpAddress = requestContext.IpAddress,
            UserAgent = requestContext.UserAgent
        }).ExecuteAffrowsAsync(cancellationToken);
    }

    private static string CreateRefreshToken(string sessionId)
    {
        return $"{sessionId}.{Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_')}";
    }

    private static string ResolveSessionId(string refreshToken)
    {
        var dotIndex = refreshToken.IndexOf('.', StringComparison.Ordinal);
        if (dotIndex <= 0)
        {
            throw new BusinessException("UNAUTHORIZED", "Refresh token is invalid.");
        }

        return refreshToken[..dotIndex];
    }
}
