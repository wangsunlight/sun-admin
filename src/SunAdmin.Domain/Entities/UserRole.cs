namespace SunAdmin.Domain.Entities;

public sealed class UserRole
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
