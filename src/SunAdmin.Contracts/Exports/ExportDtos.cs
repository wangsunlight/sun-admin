using SunAdmin.Domain.Enums;

namespace SunAdmin.Contracts.Exports;

public sealed record ExportTaskQuery(
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null,
    ExportTaskStatus? Status = null);

public sealed record ExportTaskDto(
    long Id,
    string TaskName,
    string ExportType,
    ExportTaskStatus Status,
    string? ParametersJson,
    string? FilePath,
    string? ErrorMessage,
    long CreatedByUserId,
    string CreatedByUserName,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? FinishedAt);

public sealed record CreateExportTaskRequest(
    string TaskName,
    string ExportType,
    string? ParametersJson);
