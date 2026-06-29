using SunAdmin.Contracts.Audit;
using SunAdmin.Contracts.Common;

namespace SunAdmin.Application.Abstractions;

public interface IEntityAuditService
{
    Task WriteAsync(string entityName, string entityId, string changeType, object? before, object? after, CancellationToken cancellationToken = default);
    Task<PagedResult<EntityChangeLogDto>> GetPageAsync(EntityChangeLogQuery query, CancellationToken cancellationToken = default);
}
