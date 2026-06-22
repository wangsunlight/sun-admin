using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Settings;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SettingsController(ISettingService settingService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(SystemPermissionCodes.SettingView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SettingDto>>>> GetAll(CancellationToken cancellationToken)
    {
        return ApiResponse<IReadOnlyList<SettingDto>>.Ok(await settingService.GetAllAsync(cancellationToken));
    }

    [HttpPut("{key}")]
    [RequirePermission(SystemPermissionCodes.SettingUpdate)]
    public async Task<ActionResult<ApiResponse<SettingDto>>> Update(string key, UpdateSettingRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<SettingDto>.Ok(await settingService.UpdateAsync(key, request, cancellationToken));
    }
}
