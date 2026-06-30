using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Contracts.Settings;
using SunAdmin.Domain.Entities;

namespace SunAdmin.Infrastructure.Services;

public sealed class SettingService(IFreeSql freeSql, IEntityAuditService auditService) : ISettingService
{
    public async Task<IReadOnlyList<SettingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var settings = await freeSql.Select<SystemSetting>().OrderBy(x => x.Id).ToListAsync(cancellationToken);
        return settings.Select(ToDto).ToList();
    }

    public async Task<SettingDto> UpdateAsync(string key, UpdateSettingRequest request, CancellationToken cancellationToken = default)
    {
        var setting = await freeSql.Select<SystemSetting>().Where(x => x.Key == key).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "Setting not found.");

        var before = new { setting.Key, Value = SanitizeValue(setting.Key, setting.Value) };
        setting.Value = request.Value.Trim();
        setting.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<SystemSetting>().SetSource(setting).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(SystemSetting), setting.Id.ToString(), "Update", before, new { setting.Key, Value = SanitizeValue(setting.Key, setting.Value) }, cancellationToken);
        return ToDto(setting);
    }

    private static SettingDto ToDto(SystemSetting setting)
    {
        return new SettingDto(
            setting.Id,
            setting.Key,
            setting.Value,
            setting.Name,
            setting.Description,
            setting.UpdatedAt);
    }

    private static string SanitizeValue(string key, string value)
    {
        return IsSensitiveKey(key) ? "***" : value;
    }

    private static bool IsSensitiveKey(string key)
    {
        return key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("key", StringComparison.OrdinalIgnoreCase);
    }
}
