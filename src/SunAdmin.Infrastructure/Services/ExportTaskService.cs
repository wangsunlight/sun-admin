using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Exports;
using SunAdmin.Domain.Entities;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Infrastructure.Services;

public sealed class ExportTaskService(IFreeSql freeSql, ICurrentUser currentUser, IEntityAuditService auditService) : IExportTaskService
{
    public async Task<PagedResult<ExportTaskDto>> GetPageAsync(ExportTaskQuery query, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var selector = freeSql.Select<ExportTask>();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            selector = selector.Where(x => x.TaskName.Contains(query.Keyword) || x.ExportType.Contains(query.Keyword) || x.CreatedByUserName.Contains(query.Keyword));
        }

        if (query.Status.HasValue)
        {
            selector = selector.Where(x => x.Status == query.Status.Value);
        }

        var total = await selector.CountAsync(cancellationToken);
        var tasks = await selector.OrderByDescending(x => x.Id).Page(pageIndex, pageSize).ToListAsync(cancellationToken);
        return new PagedResult<ExportTaskDto>(tasks.Select(ToDto).ToList(), total, pageIndex, pageSize);
    }

    public async Task<ExportTaskDto> CreateAsync(CreateExportTaskRequest request, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId ?? throw new BusinessException("UNAUTHORIZED", "User is not authenticated.");
        var task = new ExportTask
        {
            TaskName = request.TaskName.Trim(),
            ExportType = request.ExportType.Trim(),
            ParametersJson = request.ParametersJson,
            CreatedByUserId = userId,
            CreatedByUserName = currentUser.UserName ?? userId.ToString()
        };
        task.Id = await freeSql.Insert(task).ExecuteIdentityAsync(cancellationToken);
        await auditService.WriteAsync(nameof(ExportTask), task.Id.ToString(), "Create", null, task, cancellationToken);
        return ToDto(task);
    }

    public Task<ExportTaskDto> MarkSucceededAsync(long id, string filePath, CancellationToken cancellationToken = default)
    {
        return MarkAsync(id, ExportTaskStatus.Succeeded, filePath, null, cancellationToken);
    }

    public Task<ExportTaskDto> MarkFailedAsync(long id, string errorMessage, CancellationToken cancellationToken = default)
    {
        return MarkAsync(id, ExportTaskStatus.Failed, null, errorMessage, cancellationToken);
    }

    private async Task<ExportTaskDto> MarkAsync(long id, ExportTaskStatus status, string? filePath, string? errorMessage, CancellationToken cancellationToken)
    {
        var task = await freeSql.Select<ExportTask>().Where(x => x.Id == id).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "Export task not found.");
        task.Status = status;
        task.FilePath = filePath ?? task.FilePath;
        task.ErrorMessage = errorMessage;
        task.StartedAt ??= DateTime.UtcNow;
        task.FinishedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<ExportTask>().SetSource(task).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(ExportTask), task.Id.ToString(), "Update", null, task, cancellationToken);
        return ToDto(task);
    }

    private static ExportTaskDto ToDto(ExportTask task)
    {
        return new ExportTaskDto(task.Id, task.TaskName, task.ExportType, task.Status, task.ParametersJson, task.FilePath, task.ErrorMessage, task.CreatedByUserId, task.CreatedByUserName, task.CreatedAt, task.StartedAt, task.FinishedAt);
    }
}
