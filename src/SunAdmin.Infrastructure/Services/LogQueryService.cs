using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Logs;
using SunAdmin.Domain.Entities;

namespace SunAdmin.Infrastructure.Services;

public sealed class LogQueryService(IFreeSql freeSql) : ILogQueryService
{
    public async Task<PagedResult<OperationLogDto>> GetOperationLogsAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var selector = freeSql.Select<OperationLog>();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            selector = selector.Where(x =>
                x.UserName.Contains(query.Keyword) ||
                x.Path.Contains(query.Keyword) ||
                x.Method.Contains(query.Keyword));
        }

        if (query.Succeeded.HasValue)
        {
            selector = selector.Where(x => x.Succeeded == query.Succeeded.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            selector = selector.Where(x => x.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            selector = selector.Where(x => x.CreatedAt <= query.CreatedTo.Value);
        }

        var total = await selector.CountAsync(cancellationToken);
        var logs = await selector.OrderByDescending(x => x.Id).Page(pageIndex, pageSize).ToListAsync(cancellationToken);
        return new PagedResult<OperationLogDto>(logs.Select(ToDto).ToList(), total, pageIndex, pageSize);
    }

    public async Task<PagedResult<LoginLogDto>> GetLoginLogsAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var selector = freeSql.Select<LoginLog>();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            selector = selector.Where(x =>
                x.Account.Contains(query.Keyword) ||
                x.UserName!.Contains(query.Keyword) ||
                x.Message.Contains(query.Keyword));
        }

        if (query.Succeeded.HasValue)
        {
            selector = selector.Where(x => x.Succeeded == query.Succeeded.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            selector = selector.Where(x => x.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            selector = selector.Where(x => x.CreatedAt <= query.CreatedTo.Value);
        }

        var total = await selector.CountAsync(cancellationToken);
        var logs = await selector.OrderByDescending(x => x.Id).Page(pageIndex, pageSize).ToListAsync(cancellationToken);
        return new PagedResult<LoginLogDto>(logs.Select(ToDto).ToList(), total, pageIndex, pageSize);
    }

    public static OperationLogDto ToDto(OperationLog log)
    {
        return new OperationLogDto(
            log.Id,
            log.UserId,
            log.UserName,
            log.Method,
            log.Path,
            log.StatusCode,
            log.Succeeded,
            log.DurationMs,
            log.IpAddress,
            log.UserAgent,
            log.ErrorMessage,
            log.CreatedAt);
    }

    public static LoginLogDto ToDto(LoginLog log)
    {
        return new LoginLogDto(
            log.Id,
            log.UserId,
            log.Account,
            log.UserName,
            log.Succeeded,
            log.Message,
            log.IpAddress,
            log.UserAgent,
            log.CreatedAt);
    }
}
