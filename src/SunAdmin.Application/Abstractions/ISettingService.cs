using SunAdmin.Contracts.Settings;

namespace SunAdmin.Application.Abstractions;

public interface ISettingService
{
    Task<IReadOnlyList<SettingDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SettingDto> UpdateAsync(string key, UpdateSettingRequest request, CancellationToken cancellationToken = default);
}
