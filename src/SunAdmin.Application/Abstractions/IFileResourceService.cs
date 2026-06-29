using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Files;

namespace SunAdmin.Application.Abstractions;

public interface IFileResourceService
{
    Task<PagedResult<FileResourceDto>> GetPageAsync(FileQuery query, CancellationToken cancellationToken = default);
    Task<FileResourceDto> CreateAsync(CreateFileResourceRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
