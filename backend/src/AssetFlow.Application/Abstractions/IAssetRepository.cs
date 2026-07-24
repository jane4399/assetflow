using AssetFlow.Application.Common;
using AssetFlow.Application.Contracts.Assets;
using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Abstractions;

public interface IAssetRepository : IRepository<Asset>
{
    /// <summary>Search with the owning <see cref="Site"/> eagerly loaded.</summary>
    Task<PagedResult<Asset>> SearchAsync(AssetQuery query, CancellationToken cancellationToken = default);

    /// <summary>Fetch a single asset with its <see cref="Site"/> loaded for projection.</summary>
    Task<Asset?> GetWithSiteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> TagExistsAsync(string tag, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<bool> HasWorkOrdersAsync(Guid assetId, CancellationToken cancellationToken = default);
}
