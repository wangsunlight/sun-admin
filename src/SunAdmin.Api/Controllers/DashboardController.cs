using Microsoft.AspNetCore.Mvc;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Dashboard;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetStats(CancellationToken cancellationToken)
    {
        return ApiResponse<DashboardStatsDto>.Ok(await dashboardService.GetStatsAsync(cancellationToken));
    }
}
