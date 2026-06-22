using SunAdmin.Application.Abstractions;
using SunAdmin.Domain.Constants;
using SunAdmin.Domain.Entities;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Infrastructure.Security;

public sealed class PermissionChecker(IFreeSql freeSql) : IPermissionChecker
{
    public async Task<bool> IsSuperAdminAsync(long userId, CancellationToken cancellationToken = default)
    {
        var count = await freeSql.Select<UserRole, Role>()
            .InnerJoin((userRole, role) => userRole.RoleId == role.Id)
            .Where((userRole, role) => userRole.UserId == userId && role.Code == SystemRoleCodes.SuperAdmin && role.Status == RecordStatus.Enabled && role.DeletedAt == null)
            .CountAsync(cancellationToken);
        return count > 0;
    }

    public async Task<bool> HasPermissionAsync(long userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        if (await IsSuperAdminAsync(userId, cancellationToken))
        {
            return true;
        }

        var count = await freeSql.Select<UserRole, Role, RoleMenu, Menu>()
            .InnerJoin((userRole, role, roleMenu, menu) => userRole.RoleId == role.Id)
            .InnerJoin((userRole, role, roleMenu, menu) => role.Id == roleMenu.RoleId)
            .InnerJoin((userRole, role, roleMenu, menu) => roleMenu.MenuId == menu.Id)
            .Where((userRole, role, roleMenu, menu) =>
                userRole.UserId == userId &&
                role.Status == RecordStatus.Enabled &&
                role.DeletedAt == null &&
                menu.Status == RecordStatus.Enabled &&
                menu.DeletedAt == null &&
                menu.PermissionCode == permissionCode)
            .CountAsync(cancellationToken);
        return count > 0;
    }
}
