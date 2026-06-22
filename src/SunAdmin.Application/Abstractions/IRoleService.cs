using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Roles;

namespace SunAdmin.Application.Abstractions;

public interface IRoleService
{
    Task<PagedResult<RoleDto>> GetPageAsync(RoleQuery query, CancellationToken cancellationToken = default);
    Task<RoleDto?> GetAsync(long id, CancellationToken cancellationToken = default);
    Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleDto> UpdateAsync(long id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task AssignMenusAsync(long id, AssignRoleMenusRequest request, CancellationToken cancellationToken = default);
}
