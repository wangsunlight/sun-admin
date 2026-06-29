using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Dictionaries;

namespace SunAdmin.Application.Abstractions;

public interface IDictionaryService
{
    Task<PagedResult<DictionaryDto>> GetPageAsync(DictionaryQuery query, CancellationToken cancellationToken = default);
    Task<DictionaryDto?> GetAsync(long id, CancellationToken cancellationToken = default);
    Task<DictionaryDto> CreateAsync(CreateDictionaryRequest request, CancellationToken cancellationToken = default);
    Task<DictionaryDto> UpdateAsync(long id, UpdateDictionaryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<DictionaryItemDto> UpsertItemAsync(long dictionaryId, long? itemId, UpsertDictionaryItemRequest request, CancellationToken cancellationToken = default);
    Task DeleteItemAsync(long dictionaryId, long itemId, CancellationToken cancellationToken = default);
}
