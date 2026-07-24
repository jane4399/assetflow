using AssetFlow.Application.Common;

namespace AssetFlow.Application.Contracts.Sites;

/// <summary>Query-string filter for listing sites. Bound with <c>[FromQuery]</c>.</summary>
public class SiteQuery : PaginationQuery
{
    /// <summary>Free-text match against name or code (case-insensitive contains).</summary>
    public string? Search { get; set; }
}
