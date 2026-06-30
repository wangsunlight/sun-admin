using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Notifications;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(SystemPermissionCodes.NotificationView)]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationDto>>>> GetPage([FromQuery] NotificationQuery query, CancellationToken cancellationToken)
    {
        return ApiResponse<PagedResult<NotificationDto>>.Ok(await notificationService.GetPageAsync(query, cancellationToken));
    }

    [HttpPost]
    [RequirePermission(SystemPermissionCodes.NotificationCreate)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> Create(CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<NotificationDto>.Ok(await notificationService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{id:long}")]
    [RequirePermission(SystemPermissionCodes.NotificationUpdate)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> Update(long id, UpdateNotificationRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<NotificationDto>.Ok(await notificationService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(SystemPermissionCodes.NotificationDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(long id, CancellationToken cancellationToken)
    {
        await notificationService.DeleteAsync(id, cancellationToken);
        return ApiResponse.Ok();
    }
}
