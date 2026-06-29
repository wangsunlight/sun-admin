using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Auth;
using SunAdmin.Contracts.Common;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<LoginResponse>.Ok(await authService.LoginAsync(request, cancellationToken));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<LoginResponse>.Ok(await authService.RefreshAsync(request, cancellationToken));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout(CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<ActionResult<ApiResponse>> LogoutAll(CancellationToken cancellationToken)
    {
        await authService.LogoutAllAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<CurrentUserDto>>> Me(CancellationToken cancellationToken)
    {
        return ApiResponse<CurrentUserDto>.Ok(await authService.GetCurrentUserAsync(cancellationToken));
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<CurrentUserDto>>> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<CurrentUserDto>.Ok(await authService.UpdateProfileAsync(request, cancellationToken));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ChangePasswordAsync(request, cancellationToken);
        return ApiResponse.Ok();
    }
}
