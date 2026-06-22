namespace SunAdmin.Contracts.Common;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    long Total,
    int PageIndex,
    int PageSize);
