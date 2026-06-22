using SunAdmin.Domain.Entities;

namespace SunAdmin.Application.Abstractions;

public sealed record JwtTokenResult(string AccessToken, DateTime ExpiresAt, string SessionId);

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(User user, IReadOnlyList<string> roleCodes, string sessionId);
}
