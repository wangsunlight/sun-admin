using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Roles;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(SystemPermissionCodes.RoleView)]
    public async Task<ActionResult<ApiResponse<PagedResult<RoleDto>>>> GetPage([FromQuery] RoleQuery query, CancellationToken cancellationToken)
    {
        return ApiResponse<PagedResult<RoleDto>>.Ok(await roleService.GetPageAsync(query, cancellationToken));
    }

    [HttpGet("{id:long}")]
    [RequirePermission(SystemPermissionCodes.RoleView)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Get(long id, CancellationToken cancellationToken)
    {
        var role = await roleService.GetAsync(id, cancellationToken);
        return role is null ? NotFound(ApiResponse<RoleDto>.Fail("NOT_FOUND", "Role not found.")) : ApiResponse<RoleDto>.Ok(role);
    }

    [HttpPost]
    [RequirePermission(SystemPermissionCodes.RoleCreate)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<RoleDto>.Ok(await roleService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{id:long}")]
    [RequirePermission(SystemPermissionCodes.RoleUpdate)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(long id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<RoleDto>.Ok(await roleService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(SystemPermissionCodes.RoleDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(long id, CancellationToken cancellationToken)
    {
        await roleService.DeleteAsync(id, cancellationToken);
        return ApiResponse.Ok();
    }

    [HttpPut("{id:long}/menus")]
    [RequirePermission(SystemPermissionCodes.RoleUpdate)]
    public async Task<ActionResult<ApiResponse>> AssignMenus(long id, AssignRoleMenusRequest request, CancellationToken cancellationToken)
    {
        await roleService.AssignMenusAsync(id, request, cancellationToken);
        return ApiResponse.Ok();
    }
}
