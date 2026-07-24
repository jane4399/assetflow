using AssetFlow.Application.Common;
using AssetFlow.Application.Contracts.Sites;
using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Abstractions;

public interface ISiteRepository : IRepository<Site>
{
    Task<PagedResult<Site>> SearchAsync(SiteQuery query, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the number of assets per site for the supplied ids in a single
    /// aggregate query, avoiding an N+1 when projecting <c>SiteDto.AssetCount</c>.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> GetAssetCountsAsync(
        IReadOnlyCollection<Guid> siteIds,
        CancellationToken cancellationToken = default);

    Task<bool> HasAssetsAsync(Guid siteId, CancellationToken cancellationToken = default);
}
