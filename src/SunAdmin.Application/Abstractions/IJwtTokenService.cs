using SunAdmin.Domain.Entities;

namespace SunAdmin.Application.Abstractions;

public sealed record JwtTokenResult(string AccessToken, DateTime ExpiresAt);

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(User user, IReadOnlyList<string> roleCodes);
}
