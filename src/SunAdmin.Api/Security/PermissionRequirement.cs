using Microsoft.AspNetCore.Authorization;

namespace SunAdmin.Api.Security;

public sealed record PermissionRequirement(string PermissionCode) : IAuthorizationRequirement;
