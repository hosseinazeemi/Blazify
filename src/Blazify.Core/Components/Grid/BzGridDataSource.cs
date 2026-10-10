namespace BzCore.Components.Grid;

public sealed record BzGridDataRequest(int Page, int PageSize, string? Search,
    string? SortColumn, bool SortAscending, IReadOnlyDictionary<string, string> Filters,
    CancellationToken CancellationToken);

public sealed record BzGridDataResult<TItem>(IReadOnlyList<TItem> Items, int TotalCount);
