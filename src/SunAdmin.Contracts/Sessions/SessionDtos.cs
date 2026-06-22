namespace SunAdmin.Contracts.Sessions;

public sealed record SessionQuery(
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null,
    bool ActiveOnly = true);

public sealed record SessionDto(
    string SessionId,
    long UserId,
    string UserName,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? RevokedAt);
