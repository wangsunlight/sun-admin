using SunAdmin.Contracts.Dashboard;

namespace SunAdmin.Application.Abstractions;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
