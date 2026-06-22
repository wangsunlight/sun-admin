using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Departments;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DepartmentsController(IDepartmentService departmentService) : ControllerBase
{
    [HttpGet("tree")]
    [RequirePermission(SystemPermissionCodes.DepartmentView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DepartmentDto>>>> GetTree(CancellationToken cancellationToken)
    {
        return ApiResponse<IReadOnlyList<DepartmentDto>>.Ok(await departmentService.GetTreeAsync(cancellationToken));
    }

    [HttpGet("{id:long}")]
    [RequirePermission(SystemPermissionCodes.DepartmentView)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Get(long id, CancellationToken cancellationToken)
    {
        var department = await departmentService.GetAsync(id, cancellationToken);
        return department is null ? NotFound(ApiResponse<DepartmentDto>.Fail("NOT_FOUND", "Department not found.")) : ApiResponse<DepartmentDto>.Ok(department);
    }

    [HttpPost]
    [RequirePermission(SystemPermissionCodes.DepartmentCreate)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Create(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<DepartmentDto>.Ok(await departmentService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{id:long}")]
    [RequirePermission(SystemPermissionCodes.DepartmentUpdate)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Update(long id, UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<DepartmentDto>.Ok(await departmentService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(SystemPermissionCodes.DepartmentDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(long id, CancellationToken cancellationToken)
    {
        await departmentService.DeleteAsync(id, cancellationToken);
        return ApiResponse.Ok();
    }
}
