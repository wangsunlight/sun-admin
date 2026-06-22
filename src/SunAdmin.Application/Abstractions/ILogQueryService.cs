using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Logs;

namespace SunAdmin.Application.Abstractions;

public interface ILogQueryService
{
    Task<PagedResult<OperationLogDto>> GetOperationLogsAsync(LogQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<LoginLogDto>> GetLoginLogsAsync(LogQuery query, CancellationToken cancellationToken = default);
}
