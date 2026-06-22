namespace SunAdmin.Application.Abstractions;

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(long userId, string permissionCode, CancellationToken cancellationToken = default);
    Task<bool> IsSuperAdminAsync(long userId, CancellationToken cancellationToken = default);
}
