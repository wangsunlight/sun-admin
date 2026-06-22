using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Users;

namespace SunAdmin.Application.Abstractions;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetPageAsync(UserQuery query, CancellationToken cancellationToken = default);
    Task<UserDto?> GetAsync(long id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task SetEnabledAsync(long id, bool enabled, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(long id, ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task AssignRolesAsync(long id, AssignUserRolesRequest request, CancellationToken cancellationToken = default);
    Task BatchEnableAsync(BatchUserRequest request, bool enabled, CancellationToken cancellationToken = default);
    Task BatchDeleteAsync(BatchUserRequest request, CancellationToken cancellationToken = default);
}
