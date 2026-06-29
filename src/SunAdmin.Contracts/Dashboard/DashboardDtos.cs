using SunAdmin.Contracts.Logs;

namespace SunAdmin.Contracts.Dashboard;

public sealed record DashboardStatsDto(
    long UserCount,
    long EnabledUserCount,
    long RoleCount,
    long DepartmentCount,
    long PositionCount,
    long MenuCount,
    long OperationCountToday,
    long FailedLoginCountToday,
    long ApiErrorCountToday,
    long ActiveSessionCount,
    long PendingExportCount,
    IReadOnlyList<OperationLogDto> RecentOperations,
    IReadOnlyList<LoginLogDto> RecentLogins);
