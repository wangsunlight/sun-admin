using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Menus;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MenusController(IMenuService menuService) : ControllerBase
{
    [HttpGet("tree")]
    [RequirePermission(SystemPermissionCodes.MenuView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MenuTreeNodeDto>>>> GetTree(CancellationToken cancellationToken)
    {
        return ApiResponse<IReadOnlyList<MenuTreeNodeDto>>.Ok(await menuService.GetTreeAsync(cancellationToken));
    }

    [HttpGet("{id:long}")]
    [RequirePermission(SystemPermissionCodes.MenuView)]
    public async Task<ActionResult<ApiResponse<MenuDto>>> Get(long id, CancellationToken cancellationToken)
    {
        var menu = await menuService.GetAsync(id, cancellationToken);
        return menu is null ? NotFound(ApiResponse<MenuDto>.Fail("NOT_FOUND", "Menu not found.")) : ApiResponse<MenuDto>.Ok(menu);
    }

    [HttpPost]
    [RequirePermission(SystemPermissionCodes.MenuCreate)]
    public async Task<ActionResult<ApiResponse<MenuDto>>> Create(CreateMenuRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<MenuDto>.Ok(await menuService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{id:long}")]
    [RequirePermission(SystemPermissionCodes.MenuUpdate)]
    public async Task<ActionResult<ApiResponse<MenuDto>>> Update(long id, UpdateMenuRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<MenuDto>.Ok(await menuService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(SystemPermissionCodes.MenuDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(long id, CancellationToken cancellationToken)
    {
        await menuService.DeleteAsync(id, cancellationToken);
        return ApiResponse.Ok();
    }
}
