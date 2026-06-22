using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Positions;
using SunAdmin.Domain.Entities;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Infrastructure.Services;

public sealed class PositionService(IFreeSql freeSql) : IPositionService
{
    public async Task<PagedResult<PositionDto>> GetPageAsync(PositionQuery query, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var selector = freeSql.Select<Position>().Where(x => x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            selector = selector.Where(x => x.Code.Contains(query.Keyword) || x.Name.Contains(query.Keyword));
        }

        var total = await selector.CountAsync(cancellationToken);
        var positions = await selector.OrderBy(x => x.SortOrder).OrderByDescending(x => x.Id).Page(pageIndex, pageSize).ToListAsync(cancellationToken);
        return new PagedResult<PositionDto>(positions.Select(ToDto).ToList(), total, pageIndex, pageSize);
    }

    public async Task<PositionDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var position = await freeSql.Select<Position>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken);
        return position is null ? null : ToDto(position);
    }

    public async Task<PositionDto> CreateAsync(CreatePositionRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureCodeAvailableAsync(request.Code, null, cancellationToken);
        var position = new Position
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            SortOrder = request.SortOrder
        };
        position.Id = await freeSql.Insert(position).ExecuteIdentityAsync(cancellationToken);
        return ToDto(position);
    }

    public async Task<PositionDto> UpdateAsync(long id, UpdatePositionRequest request, CancellationToken cancellationToken = default)
    {
        var position = await GetEntityAsync(id, cancellationToken);
        if (position.IsBuiltIn && request.Status == RecordStatus.Disabled)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in position cannot be disabled.");
        }

        await EnsureCodeAvailableAsync(request.Code, id, cancellationToken);
        position.Code = request.Code;
        position.Name = request.Name;
        position.Description = request.Description;
        position.SortOrder = request.SortOrder;
        position.Status = request.Status;
        position.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<Position>().SetSource(position).ExecuteAffrowsAsync(cancellationToken);
        return ToDto(position);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var position = await GetEntityAsync(id, cancellationToken);
        if (position.IsBuiltIn)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in position cannot be deleted.");
        }

        position.DeletedAt = DateTime.UtcNow;
        await freeSql.Update<Position>().SetSource(position).ExecuteAffrowsAsync(cancellationToken);
    }

    private async Task<Position> GetEntityAsync(long id, CancellationToken cancellationToken)
    {
        return await freeSql.Select<Position>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "Position not found.");
    }

    private async Task EnsureCodeAvailableAsync(string code, long? currentId, CancellationToken cancellationToken)
    {
        var exists = await freeSql.Select<Position>()
            .Where(x => x.DeletedAt == null && x.Code == code && (!currentId.HasValue || x.Id != currentId.Value))
            .AnyAsync(cancellationToken);
        if (exists)
        {
            throw new BusinessException("CONFLICT", "Position code already exists.");
        }
    }

    private static PositionDto ToDto(Position position)
    {
        return new PositionDto(position.Id, position.Code, position.Name, position.Description, position.SortOrder, position.Status, position.IsBuiltIn, position.CreatedAt);
    }
}
