using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Dashboard;
using SunAdmin.Domain.Entities;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Infrastructure.Services;

public sealed class DashboardService(IFreeSql freeSql) : IDashboardService
{
    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var recentOperations = await freeSql.Select<OperationLog>()
            .OrderByDescending(x => x.Id)
            .Limit(6)
            .ToListAsync(cancellationToken);
        var recentLogins = await freeSql.Select<LoginLog>()
            .OrderByDescending(x => x.Id)
            .Limit(6)
            .ToListAsync(cancellationToken);

        return new DashboardStatsDto(
            await freeSql.Select<User>().Where(x => x.DeletedAt == null).CountAsync(cancellationToken),
            await freeSql.Select<User>().Where(x => x.DeletedAt == null && x.Status == RecordStatus.Enabled).CountAsync(cancellationToken),
            await freeSql.Select<Role>().Where(x => x.DeletedAt == null).CountAsync(cancellationToken),
            await freeSql.Select<Department>().Where(x => x.DeletedAt == null).CountAsync(cancellationToken),
            await freeSql.Select<Position>().Where(x => x.DeletedAt == null).CountAsync(cancellationToken),
            await freeSql.Select<Menu>().Where(x => x.DeletedAt == null).CountAsync(cancellationToken),
            await freeSql.Select<OperationLog>().Where(x => x.CreatedAt >= today).CountAsync(cancellationToken),
            await freeSql.Select<LoginLog>().Where(x => x.CreatedAt >= today && !x.Succeeded).CountAsync(cancellationToken),
            await freeSql.Select<OperationLog>().Where(x => x.CreatedAt >= today && x.StatusCode >= 500).CountAsync(cancellationToken),
            await freeSql.Select<LoginSession>().Where(x => x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow).CountAsync(cancellationToken),
            await freeSql.Select<ExportTask>().Where(x => x.Status == ExportTaskStatus.Pending || x.Status == ExportTaskStatus.Running).CountAsync(cancellationToken),
            recentOperations.Select(LogQueryService.ToDto).ToList(),
            recentLogins.Select(LogQueryService.ToDto).ToList());
    }
}
