namespace SunAdmin.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public bool SyncStructure { get; set; } = true;
    public bool SeedData { get; set; } = true;
    public bool DisableInitializer { get; set; }
}
