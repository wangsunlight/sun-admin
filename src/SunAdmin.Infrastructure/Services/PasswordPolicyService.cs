using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Domain.Entities;

namespace SunAdmin.Infrastructure.Services;

public sealed class PasswordPolicyService(IFreeSql freeSql) : IPasswordPolicyService
{
    public void Validate(string password)
    {
        var settings = freeSql.Select<SystemSetting>()
            .Where(x => x.Key.StartsWith("security.password."))
            .ToList(x => new { x.Key, x.Value })
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        var minLength = GetInt(settings, "security.password.minLength", 8);
        if (password.Length < minLength)
        {
            throw new BusinessException("VALIDATION_ERROR", $"Password must be at least {minLength} characters.");
        }

        if (GetBool(settings, "security.password.requireDigit", true) && !password.Any(char.IsDigit))
        {
            throw new BusinessException("VALIDATION_ERROR", "Password must contain a digit.");
        }

        if (GetBool(settings, "security.password.requireUppercase", true) && !password.Any(char.IsUpper))
        {
            throw new BusinessException("VALIDATION_ERROR", "Password must contain an uppercase letter.");
        }

        if (GetBool(settings, "security.password.requireLowercase", true) && !password.Any(char.IsLower))
        {
            throw new BusinessException("VALIDATION_ERROR", "Password must contain a lowercase letter.");
        }

        if (GetBool(settings, "security.password.requireNonAlphanumeric", false) && password.All(char.IsLetterOrDigit))
        {
            throw new BusinessException("VALIDATION_ERROR", "Password must contain a symbol.");
        }
    }

    private static int GetInt(IReadOnlyDictionary<string, string> settings, string key, int fallback)
    {
        return settings.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static bool GetBool(IReadOnlyDictionary<string, string> settings, string key, bool fallback)
    {
        return settings.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : fallback;
    }
}
