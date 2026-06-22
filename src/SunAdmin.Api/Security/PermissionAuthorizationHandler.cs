using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SunAdmin.Application.Abstractions;

namespace SunAdmin.Api.Security;

public sealed class PermissionAuthorizationHandler(IPermissionChecker permissionChecker) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdValue, out var userId))
        {
            return;
        }

        if (await permissionChecker.HasPermissionAsync(userId, requirement.PermissionCode))
        {
            context.Succeed(requirement);
        }
    }
}
