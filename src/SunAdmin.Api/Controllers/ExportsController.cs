using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Exports;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ExportsController(IExportTaskService exportTaskService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(SystemPermissionCodes.ExportView)]
    public async Task<ActionResult<ApiResponse<PagedResult<ExportTaskDto>>>> GetPage([FromQuery] ExportTaskQuery query, CancellationToken cancellationToken)
    {
        return ApiResponse<PagedResult<ExportTaskDto>>.Ok(await exportTaskService.GetPageAsync(query, cancellationToken));
    }

    [HttpPost]
    [RequirePermission(SystemPermissionCodes.ExportCreate)]
    public async Task<ActionResult<ApiResponse<ExportTaskDto>>> Create(CreateExportTaskRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<ExportTaskDto>.Ok(await exportTaskService.CreateAsync(request, cancellationToken));
    }
}
