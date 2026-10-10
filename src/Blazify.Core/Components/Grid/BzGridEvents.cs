namespace BzCore.Components.Grid;

/// <summary>Raised by <see cref="BzGrid{TItem}"/> when the sort column/direction changes.</summary>
public sealed record BzGridSortEvent(string? Field, bool Ascending);

public sealed record BzGridPageEvent(int Page, int PageSize, int TotalCount);

public sealed record BzGridFilterEvent(string? Field, string? Value);
