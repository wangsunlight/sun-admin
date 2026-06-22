namespace SunAdmin.Application.Abstractions;

public interface ICurrentUser
{
    long? UserId { get; }
    string? UserName { get; }
    string? SessionId { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
}
