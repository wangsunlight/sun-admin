using System.Security.Claims;
using SunAdmin.Application.Abstractions;

namespace SunAdmin.Api.Security;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public long? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(value, out var id) ? id : null;
        }
    }

    public string? UserName => httpContextAccessor.HttpContext?.User.Identity?.Name;

    public string? SessionId => httpContextAccessor.HttpContext?.User.FindFirstValue("sid");

    public IReadOnlyList<string> Roles => httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList() ?? [];

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
