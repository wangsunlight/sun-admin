using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Exports;

namespace SunAdmin.Application.Abstractions;

public interface IExportTaskService
{
    Task<PagedResult<ExportTaskDto>> GetPageAsync(ExportTaskQuery query, CancellationToken cancellationToken = default);
    Task<ExportTaskDto> CreateAsync(CreateExportTaskRequest request, CancellationToken cancellationToken = default);
    Task<ExportTaskDto> MarkSucceededAsync(long id, string filePath, CancellationToken cancellationToken = default);
    Task<ExportTaskDto> MarkFailedAsync(long id, string errorMessage, CancellationToken cancellationToken = default);
}
