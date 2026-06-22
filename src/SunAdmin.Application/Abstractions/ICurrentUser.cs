namespace SunAdmin.Application.Abstractions;

public interface ICurrentUser
{
    long? UserId { get; }
    string? UserName { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
}
