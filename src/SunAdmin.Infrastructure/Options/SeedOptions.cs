namespace SunAdmin.Infrastructure.Options;

public sealed class SeedOptions
{
    public string AdminUserName { get; set; } = "admin";
    public string AdminEmail { get; set; } = "admin@sun-admin.local";
    public string AdminPassword { get; set; } = "Admin@123456";
}
