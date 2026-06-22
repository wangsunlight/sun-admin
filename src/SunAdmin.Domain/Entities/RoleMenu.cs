namespace SunAdmin.Domain.Entities;

public sealed class RoleMenu
{
    public long Id { get; set; }
    public long RoleId { get; set; }
    public long MenuId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
