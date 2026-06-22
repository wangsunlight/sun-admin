using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Sessions;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SessionsController(ISessionService sessionService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(SystemPermissionCodes.SessionView)]
    public async Task<ActionResult<ApiResponse<PagedResult<SessionDto>>>> GetPage([FromQuery] SessionQuery query, CancellationToken cancellationToken)
    {
        return ApiResponse<PagedResult<SessionDto>>.Ok(await sessionService.GetPageAsync(query, cancellationToken));
    }

    [HttpPost("{sessionId}/revoke")]
    [RequirePermission(SystemPermissionCodes.SessionRevoke)]
    public async Task<ActionResult<ApiResponse>> Revoke(string sessionId, CancellationToken cancellationToken)
    {
        await sessionService.RevokeAsync(sessionId, cancellationToken);
        return ApiResponse.Ok();
    }
}
