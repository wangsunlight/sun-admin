using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Sessions;

namespace SunAdmin.Application.Abstractions;

public interface ISessionService
{
    Task<PagedResult<SessionDto>> GetPageAsync(SessionQuery query, CancellationToken cancellationToken = default);
    Task RevokeAsync(string sessionId, CancellationToken cancellationToken = default);
}
