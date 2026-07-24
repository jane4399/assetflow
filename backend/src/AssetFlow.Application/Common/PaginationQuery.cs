namespace AssetFlow.Application.Common;

/// <summary>
/// Base class for query-string filter objects. Clamps page/size to safe bounds
/// so a caller can never request page 0 or exhaust the server with an unbounded
/// page size. Sorting is expressed as a field name plus direction; each service
/// maps <see cref="SortBy"/> through an allow-list to avoid arbitrary ordering.
/// </summary>
public abstract class PaginationQuery
{
    public const int MaxPageSize = 100;

    private int _page = 1;
    private int _pageSize = 20;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 1,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    /// <summary>Field to sort by (allow-listed per resource). Case-insensitive.</summary>
    public string? SortBy { get; set; }

    /// <summary>"asc" or "desc" (default ascending).</summary>
    public string? SortDir { get; set; }

    public bool IsDescending =>
        string.Equals(SortDir, "desc", StringComparison.OrdinalIgnoreCase);

    public int Skip => (Page - 1) * PageSize;
}
