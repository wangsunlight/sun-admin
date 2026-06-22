using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Users;
using SunAdmin.Domain.Entities;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Infrastructure.Services;

public sealed class UserService(IFreeSql freeSql, IPasswordHasher passwordHasher) : IUserService
{
    public async Task<PagedResult<UserDto>> GetPageAsync(UserQuery query, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var selector = freeSql.Select<User>().Where(x => x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            selector = selector.Where(x => x.UserName.Contains(query.Keyword) || x.DisplayName.Contains(query.Keyword) || x.Email.Contains(query.Keyword));
        }

        var total = await selector.CountAsync(cancellationToken);
        var users = await selector.OrderByDescending(x => x.Id).Page(pageIndex, pageSize).ToListAsync(cancellationToken);
        var items = new List<UserDto>();
        foreach (var user in users)
        {
            items.Add(await ToDtoAsync(user, cancellationToken));
        }

        return new PagedResult<UserDto>(items, total, pageIndex, pageSize);
    }

    public async Task<UserDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await freeSql.Select<User>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken);
        return user is null ? null : await ToDtoAsync(user, cancellationToken);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (await freeSql.Select<User>().Where(x => x.DeletedAt == null && (x.UserName == request.UserName || x.Email == request.Email)).AnyAsync(cancellationToken))
        {
            throw new BusinessException("CONFLICT", "User name or email already exists.");
        }

        var user = new User
        {
            UserName = request.UserName,
            DisplayName = request.DisplayName,
            Email = request.Email,
            PasswordHash = passwordHasher.HashPassword(request.Password)
        };
        user.Id = await freeSql.Insert(user).ExecuteIdentityAsync(cancellationToken);
        await ReplaceRolesAsync(user.Id, request.RoleIds ?? Array.Empty<long>(), cancellationToken);
        return await ToDtoAsync(user, cancellationToken);
    }

    public async Task<UserDto> UpdateAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetEntityAsync(id, cancellationToken);
        if (await freeSql.Select<User>().Where(x => x.DeletedAt == null && x.Id != id && x.Email == request.Email).AnyAsync(cancellationToken))
        {
            throw new BusinessException("CONFLICT", "Email already exists.");
        }

        if (user.IsBuiltIn && request.Status == RecordStatus.Disabled)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in user cannot be disabled.");
        }

        user.DisplayName = request.DisplayName;
        user.Email = request.Email;
        user.Status = request.Status;
        user.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<User>().SetSource(user).ExecuteAffrowsAsync(cancellationToken);
        return await ToDtoAsync(user, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await GetEntityAsync(id, cancellationToken);
        if (user.IsBuiltIn)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in user cannot be deleted.");
        }

        user.DeletedAt = DateTime.UtcNow;
        await freeSql.Update<User>().SetSource(user).ExecuteAffrowsAsync(cancellationToken);
    }

    public async Task SetEnabledAsync(long id, bool enabled, CancellationToken cancellationToken = default)
    {
        var user = await GetEntityAsync(id, cancellationToken);
        if (user.IsBuiltIn && !enabled)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in user cannot be disabled.");
        }

        user.Status = enabled ? RecordStatus.Enabled : RecordStatus.Disabled;
        user.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<User>().SetSource(user).ExecuteAffrowsAsync(cancellationToken);
    }

    public async Task ResetPasswordAsync(long id, ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetEntityAsync(id, cancellationToken);
        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<User>().SetSource(user).ExecuteAffrowsAsync(cancellationToken);
    }

    public Task AssignRolesAsync(long id, AssignUserRolesRequest request, CancellationToken cancellationToken = default)
    {
        return ReplaceRolesAsync(id, request.RoleIds, cancellationToken);
    }

    private async Task<User> GetEntityAsync(long id, CancellationToken cancellationToken)
    {
        return await freeSql.Select<User>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "User not found.");
    }

    private async Task ReplaceRolesAsync(long userId, IReadOnlyList<long> roleIds, CancellationToken cancellationToken)
    {
        await freeSql.Delete<UserRole>().Where(x => x.UserId == userId).ExecuteAffrowsAsync(cancellationToken);
        if (roleIds.Count > 0)
        {
            await freeSql.Insert(roleIds.Distinct().Select(roleId => new UserRole { UserId = userId, RoleId = roleId })).ExecuteAffrowsAsync(cancellationToken);
        }
    }

    private async Task<UserDto> ToDtoAsync(User user, CancellationToken cancellationToken)
    {
        var roles = await freeSql.Select<UserRole, Role>()
            .InnerJoin((userRole, role) => userRole.RoleId == role.Id)
            .Where((userRole, role) => userRole.UserId == user.Id && role.DeletedAt == null)
            .ToListAsync((userRole, role) => role.Code, cancellationToken);
        return new UserDto(user.Id, user.UserName, user.DisplayName, user.Email, user.Status, user.IsBuiltIn, user.CreatedAt, user.LastLoginAt, roles);
    }
}
