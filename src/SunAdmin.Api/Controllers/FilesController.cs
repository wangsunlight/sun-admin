using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Files;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FilesController(IFileResourceService fileResourceService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(SystemPermissionCodes.FileView)]
    public async Task<ActionResult<ApiResponse<PagedResult<FileResourceDto>>>> GetPage([FromQuery] FileQuery query, CancellationToken cancellationToken)
    {
        return ApiResponse<PagedResult<FileResourceDto>>.Ok(await fileResourceService.GetPageAsync(query, cancellationToken));
    }

    [HttpPost]
    [RequirePermission(SystemPermissionCodes.FileCreate)]
    public async Task<ActionResult<ApiResponse<FileResourceDto>>> Create(CreateFileResourceRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<FileResourceDto>.Ok(await fileResourceService.CreateAsync(request, cancellationToken));
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(SystemPermissionCodes.FileDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(long id, CancellationToken cancellationToken)
    {
        await fileResourceService.DeleteAsync(id, cancellationToken);
        return ApiResponse.Ok();
    }
}
