using AssetFlow.Application.Common;
using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Contracts.Assets;

/// <summary>Query-string filter for listing assets. Bound with <c>[FromQuery]</c>.</summary>
public class AssetQuery : PaginationQuery
{
    public AssetStatus? Status { get; set; }

    public Guid? SiteId { get; set; }

    /// <summary>Free-text match against name or tag (case-insensitive contains).</summary>
    public string? Search { get; set; }
}
