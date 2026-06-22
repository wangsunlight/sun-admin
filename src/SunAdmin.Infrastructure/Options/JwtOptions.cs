namespace SunAdmin.Infrastructure.Options;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "sun-admin";
    public string Audience { get; set; } = "sun-admin";
    public string Secret { get; set; } = "sun-admin-development-secret-change-me-32";
    public int AccessTokenMinutes { get; set; } = 120;
}
