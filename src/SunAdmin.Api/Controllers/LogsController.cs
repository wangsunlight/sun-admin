using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Logs;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LogsController(ILogQueryService logQueryService) : ControllerBase
{
    [HttpGet("operations")]
    [RequirePermission(SystemPermissionCodes.OperationLogView)]
    public async Task<ActionResult<ApiResponse<PagedResult<OperationLogDto>>>> GetOperations([FromQuery] LogQuery query, CancellationToken cancellationToken)
    {
        return ApiResponse<PagedResult<OperationLogDto>>.Ok(await logQueryService.GetOperationLogsAsync(query, cancellationToken));
    }

    [HttpGet("logins")]
    [RequirePermission(SystemPermissionCodes.LoginLogView)]
    public async Task<ActionResult<ApiResponse<PagedResult<LoginLogDto>>>> GetLogins([FromQuery] LogQuery query, CancellationToken cancellationToken)
    {
        return ApiResponse<PagedResult<LoginLogDto>>.Ok(await logQueryService.GetLoginLogsAsync(query, cancellationToken));
    }
}
