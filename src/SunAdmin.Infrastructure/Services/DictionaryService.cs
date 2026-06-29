using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Dictionaries;
using SunAdmin.Domain.Entities;

namespace SunAdmin.Infrastructure.Services;

public sealed class DictionaryService(IFreeSql freeSql, IEntityAuditService auditService) : IDictionaryService
{
    public async Task<PagedResult<DictionaryDto>> GetPageAsync(DictionaryQuery query, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var selector = freeSql.Select<DataDictionary>().Where(x => x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            selector = selector.Where(x => x.Code.Contains(query.Keyword) || x.Name.Contains(query.Keyword));
        }

        var total = await selector.CountAsync(cancellationToken);
        var dictionaries = await selector.OrderByDescending(x => x.Id).Page(pageIndex, pageSize).ToListAsync(cancellationToken);
        var items = new List<DictionaryDto>();
        foreach (var dictionary in dictionaries)
        {
            items.Add(await ToDtoAsync(dictionary, cancellationToken));
        }

        return new PagedResult<DictionaryDto>(items, total, pageIndex, pageSize);
    }

    public async Task<DictionaryDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var dictionary = await freeSql.Select<DataDictionary>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken);
        return dictionary is null ? null : await ToDtoAsync(dictionary, cancellationToken);
    }

    public async Task<DictionaryDto> CreateAsync(CreateDictionaryRequest request, CancellationToken cancellationToken = default)
    {
        if (await freeSql.Select<DataDictionary>().Where(x => x.DeletedAt == null && x.Code == request.Code).AnyAsync(cancellationToken))
        {
            throw new BusinessException("CONFLICT", "Dictionary code already exists.");
        }

        var dictionary = new DataDictionary { Code = request.Code.Trim(), Name = request.Name.Trim(), Description = request.Description };
        dictionary.Id = await freeSql.Insert(dictionary).ExecuteIdentityAsync(cancellationToken);
        await auditService.WriteAsync(nameof(DataDictionary), dictionary.Id.ToString(), "Create", null, dictionary, cancellationToken);
        return await ToDtoAsync(dictionary, cancellationToken);
    }

    public async Task<DictionaryDto> UpdateAsync(long id, UpdateDictionaryRequest request, CancellationToken cancellationToken = default)
    {
        var dictionary = await GetEntityAsync(id, cancellationToken);
        if (dictionary.IsBuiltIn && request.Status != dictionary.Status)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in dictionary status cannot be changed.");
        }

        var before = Clone(dictionary);
        dictionary.Name = request.Name.Trim();
        dictionary.Description = request.Description;
        dictionary.Status = request.Status;
        dictionary.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<DataDictionary>().SetSource(dictionary).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(DataDictionary), dictionary.Id.ToString(), "Update", before, dictionary, cancellationToken);
        return await ToDtoAsync(dictionary, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var dictionary = await GetEntityAsync(id, cancellationToken);
        if (dictionary.IsBuiltIn)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in dictionary cannot be deleted.");
        }

        dictionary.DeletedAt = DateTime.UtcNow;
        await freeSql.Update<DataDictionary>().SetSource(dictionary).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(DataDictionary), dictionary.Id.ToString(), "Delete", dictionary, null, cancellationToken);
    }

    public async Task<DictionaryItemDto> UpsertItemAsync(long dictionaryId, long? itemId, UpsertDictionaryItemRequest request, CancellationToken cancellationToken = default)
    {
        _ = await GetEntityAsync(dictionaryId, cancellationToken);
        DataDictionaryItem item;
        object? before = null;
        var changeType = "Create";
        if (itemId.HasValue)
        {
            item = await freeSql.Select<DataDictionaryItem>().Where(x => x.Id == itemId.Value && x.DictionaryId == dictionaryId && x.DeletedAt == null).FirstAsync(cancellationToken)
                ?? throw new BusinessException("NOT_FOUND", "Dictionary item not found.");
            if (await freeSql.Select<DataDictionaryItem>().Where(x => x.DictionaryId == dictionaryId && x.DeletedAt == null && x.Id != item.Id && x.Value == request.Value).AnyAsync(cancellationToken))
            {
                throw new BusinessException("CONFLICT", "Dictionary item value already exists.");
            }

            before = Clone(item);
            changeType = "Update";
        }
        else
        {
            if (await freeSql.Select<DataDictionaryItem>().Where(x => x.DictionaryId == dictionaryId && x.DeletedAt == null && x.Value == request.Value).AnyAsync(cancellationToken))
            {
                throw new BusinessException("CONFLICT", "Dictionary item value already exists.");
            }

            item = new DataDictionaryItem { DictionaryId = dictionaryId };
        }

        item.Label = request.Label.Trim();
        item.Value = request.Value.Trim();
        item.SortOrder = request.SortOrder;
        item.Status = request.Status;
        item.UpdatedAt = DateTime.UtcNow;
        if (itemId.HasValue)
        {
            await freeSql.Update<DataDictionaryItem>().SetSource(item).ExecuteAffrowsAsync(cancellationToken);
        }
        else
        {
            item.Id = await freeSql.Insert(item).ExecuteIdentityAsync(cancellationToken);
        }

        await auditService.WriteAsync(nameof(DataDictionaryItem), item.Id.ToString(), changeType, before, item, cancellationToken);
        return ToDto(item);
    }

    public async Task DeleteItemAsync(long dictionaryId, long itemId, CancellationToken cancellationToken = default)
    {
        var item = await freeSql.Select<DataDictionaryItem>().Where(x => x.Id == itemId && x.DictionaryId == dictionaryId && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "Dictionary item not found.");
        if (item.IsBuiltIn)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in dictionary item cannot be deleted.");
        }

        item.DeletedAt = DateTime.UtcNow;
        await freeSql.Update<DataDictionaryItem>().SetSource(item).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(DataDictionaryItem), item.Id.ToString(), "Delete", item, null, cancellationToken);
    }

    private async Task<DataDictionary> GetEntityAsync(long id, CancellationToken cancellationToken)
    {
        return await freeSql.Select<DataDictionary>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "Dictionary not found.");
    }

    private async Task<DictionaryDto> ToDtoAsync(DataDictionary dictionary, CancellationToken cancellationToken)
    {
        var items = await freeSql.Select<DataDictionaryItem>()
            .Where(x => x.DictionaryId == dictionary.Id && x.DeletedAt == null)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);
        return new DictionaryDto(
            dictionary.Id,
            dictionary.Code,
            dictionary.Name,
            dictionary.Description,
            dictionary.Status,
            dictionary.IsBuiltIn,
            dictionary.CreatedAt,
            items.Select(ToDto).ToList());
    }

    private static DictionaryItemDto ToDto(DataDictionaryItem item)
    {
        return new DictionaryItemDto(item.Id, item.DictionaryId, item.Label, item.Value, item.SortOrder, item.Status, item.IsBuiltIn);
    }

    private static DataDictionary Clone(DataDictionary value)
    {
        return new DataDictionary
        {
            Id = value.Id,
            Code = value.Code,
            Name = value.Name,
            Description = value.Description,
            Status = value.Status,
            IsBuiltIn = value.IsBuiltIn,
            CreatedAt = value.CreatedAt,
            UpdatedAt = value.UpdatedAt,
            DeletedAt = value.DeletedAt
        };
    }

    private static DataDictionaryItem Clone(DataDictionaryItem value)
    {
        return new DataDictionaryItem
        {
            Id = value.Id,
            DictionaryId = value.DictionaryId,
            Label = value.Label,
            Value = value.Value,
            SortOrder = value.SortOrder,
            Status = value.Status,
            IsBuiltIn = value.IsBuiltIn,
            CreatedAt = value.CreatedAt,
            UpdatedAt = value.UpdatedAt,
            DeletedAt = value.DeletedAt
        };
    }
}
