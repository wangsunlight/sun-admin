namespace SunAdmin.Contracts.Settings;

public sealed record SettingDto(
    long Id,
    string Key,
    string Value,
    string Name,
    string? Description,
    DateTime UpdatedAt);

public sealed record UpdateSettingRequest(string Value);
