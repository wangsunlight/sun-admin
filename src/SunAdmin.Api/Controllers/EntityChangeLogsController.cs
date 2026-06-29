using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Audit;
using SunAdmin.Contracts.Common;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/entity-change-logs")]
public sealed class EntityChangeLogsController(IEntityAuditService entityAuditService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(SystemPermissionCodes.EntityChangeLogView)]
    public async Task<ActionResult<ApiResponse<PagedResult<EntityChangeLogDto>>>> GetPage([FromQuery] EntityChangeLogQuery query, CancellationToken cancellationToken)
    {
        return ApiResponse<PagedResult<EntityChangeLogDto>>.Ok(await entityAuditService.GetPageAsync(query, cancellationToken));
    }
}
