using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Positions;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PositionsController(IPositionService positionService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(SystemPermissionCodes.PositionView)]
    public async Task<ActionResult<ApiResponse<PagedResult<PositionDto>>>> GetPage([FromQuery] PositionQuery query, CancellationToken cancellationToken)
    {
        return ApiResponse<PagedResult<PositionDto>>.Ok(await positionService.GetPageAsync(query, cancellationToken));
    }

    [HttpGet("{id:long}")]
    [RequirePermission(SystemPermissionCodes.PositionView)]
    public async Task<ActionResult<ApiResponse<PositionDto>>> Get(long id, CancellationToken cancellationToken)
    {
        var position = await positionService.GetAsync(id, cancellationToken);
        return position is null ? NotFound(ApiResponse<PositionDto>.Fail("NOT_FOUND", "Position not found.")) : ApiResponse<PositionDto>.Ok(position);
    }

    [HttpPost]
    [RequirePermission(SystemPermissionCodes.PositionCreate)]
    public async Task<ActionResult<ApiResponse<PositionDto>>> Create(CreatePositionRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<PositionDto>.Ok(await positionService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{id:long}")]
    [RequirePermission(SystemPermissionCodes.PositionUpdate)]
    public async Task<ActionResult<ApiResponse<PositionDto>>> Update(long id, UpdatePositionRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<PositionDto>.Ok(await positionService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(SystemPermissionCodes.PositionDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(long id, CancellationToken cancellationToken)
    {
        await positionService.DeleteAsync(id, cancellationToken);
        return ApiResponse.Ok();
    }
}
