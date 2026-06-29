using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Sessions;
using SunAdmin.Domain.Entities;

namespace SunAdmin.Infrastructure.Services;

public sealed class SessionService(IFreeSql freeSql) : ISessionService
{
    public async Task<PagedResult<SessionDto>> GetPageAsync(SessionQuery query, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var now = DateTime.UtcNow;
        var selector = freeSql.Select<LoginSession>();

        if (query.ActiveOnly)
        {
            selector = selector.Where(x => x.RevokedAt == null && x.ExpiresAt > now);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            selector = selector.Where(x =>
                x.UserName.Contains(query.Keyword) ||
                x.SessionId.Contains(query.Keyword) ||
                x.IpAddress!.Contains(query.Keyword));
        }

        var total = await selector.CountAsync(cancellationToken);
        var sessions = await selector.OrderByDescending(x => x.Id).Page(pageIndex, pageSize).ToListAsync(cancellationToken);
        var items = sessions.Select(x => new SessionDto(
            x.SessionId,
            x.UserId,
            x.UserName,
            x.IpAddress,
            x.UserAgent,
            x.CreatedAt,
            x.ExpiresAt,
            x.RefreshTokenExpiresAt,
            x.LastSeenAt,
            x.RevokedAt)).ToList();

        return new PagedResult<SessionDto>(items, total, pageIndex, pageSize);
    }

    public async Task RevokeAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var session = await freeSql.Select<LoginSession>().Where(x => x.SessionId == sessionId).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "Session not found.");
        if (session.RevokedAt is null)
        {
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedReason = "admin_revoke";
            session.UpdatedAt = DateTime.UtcNow;
            await freeSql.Update<LoginSession>().SetSource(session).ExecuteAffrowsAsync(cancellationToken);
        }
    }
}
