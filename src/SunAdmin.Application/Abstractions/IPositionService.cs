using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Positions;

namespace SunAdmin.Application.Abstractions;

public interface IPositionService
{
    Task<PagedResult<PositionDto>> GetPageAsync(PositionQuery query, CancellationToken cancellationToken = default);
    Task<PositionDto?> GetAsync(long id, CancellationToken cancellationToken = default);
    Task<PositionDto> CreateAsync(CreatePositionRequest request, CancellationToken cancellationToken = default);
    Task<PositionDto> UpdateAsync(long id, UpdatePositionRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
