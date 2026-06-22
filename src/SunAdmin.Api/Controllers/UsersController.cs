using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Users;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(SystemPermissionCodes.UserView)]
    public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetPage([FromQuery] UserQuery query, CancellationToken cancellationToken)
    {
        return ApiResponse<PagedResult<UserDto>>.Ok(await userService.GetPageAsync(query, cancellationToken));
    }

    [HttpGet("{id:long}")]
    [RequirePermission(SystemPermissionCodes.UserView)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Get(long id, CancellationToken cancellationToken)
    {
        var user = await userService.GetAsync(id, cancellationToken);
        return user is null ? NotFound(ApiResponse<UserDto>.Fail("NOT_FOUND", "User not found.")) : ApiResponse<UserDto>.Ok(user);
    }

    [HttpPost]
    [RequirePermission(SystemPermissionCodes.UserCreate)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<UserDto>.Ok(await userService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{id:long}")]
    [RequirePermission(SystemPermissionCodes.UserUpdate)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(long id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<UserDto>.Ok(await userService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(SystemPermissionCodes.UserDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(long id, CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(id, cancellationToken);
        return ApiResponse.Ok();
    }

    [HttpPost("{id:long}/enable")]
    [RequirePermission(SystemPermissionCodes.UserUpdate)]
    public async Task<ActionResult<ApiResponse>> Enable(long id, CancellationToken cancellationToken)
    {
        await userService.SetEnabledAsync(id, true, cancellationToken);
        return ApiResponse.Ok();
    }

    [HttpPost("{id:long}/disable")]
    [RequirePermission(SystemPermissionCodes.UserUpdate)]
    public async Task<ActionResult<ApiResponse>> Disable(long id, CancellationToken cancellationToken)
    {
        await userService.SetEnabledAsync(id, false, cancellationToken);
        return ApiResponse.Ok();
    }

    [HttpPost("{id:long}/reset-password")]
    [RequirePermission(SystemPermissionCodes.UserUpdate)]
    public async Task<ActionResult<ApiResponse>> ResetPassword(long id, ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await userService.ResetPasswordAsync(id, request, cancellationToken);
        return ApiResponse.Ok();
    }

    [HttpPut("{id:long}/roles")]
    [RequirePermission(SystemPermissionCodes.UserUpdate)]
    public async Task<ActionResult<ApiResponse>> AssignRoles(long id, AssignUserRolesRequest request, CancellationToken cancellationToken)
    {
        await userService.AssignRolesAsync(id, request, cancellationToken);
        return ApiResponse.Ok();
    }
}
