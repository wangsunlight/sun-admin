using Microsoft.AspNetCore.Authorization;

namespace SunAdmin.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public RequirePermissionAttribute(string permissionCode)
    {
        PermissionCode = permissionCode;
        Policy = PolicyPrefix + permissionCode;
    }

    public string PermissionCode { get; }
}
