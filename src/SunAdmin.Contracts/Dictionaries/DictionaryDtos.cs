using SunAdmin.Domain.Enums;

namespace SunAdmin.Contracts.Dictionaries;

public sealed record DictionaryDto(
    long Id,
    string Code,
    string Name,
    string? Description,
    RecordStatus Status,
    bool IsBuiltIn,
    DateTime CreatedAt,
    IReadOnlyList<DictionaryItemDto> Items);

public sealed record DictionaryItemDto(
    long Id,
    long DictionaryId,
    string Label,
    string Value,
    int SortOrder,
    RecordStatus Status,
    bool IsBuiltIn);

public sealed record DictionaryQuery(
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null);

public sealed record CreateDictionaryRequest(
    string Code,
    string Name,
    string? Description);

public sealed record UpdateDictionaryRequest(
    string Name,
    string? Description,
    RecordStatus Status);

public sealed record UpsertDictionaryItemRequest(
    string Label,
    string Value,
    int SortOrder,
    RecordStatus Status);
