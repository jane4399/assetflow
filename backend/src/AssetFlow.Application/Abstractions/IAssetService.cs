using AssetFlow.Application.Common;
using AssetFlow.Application.Contracts.Assets;

namespace AssetFlow.Application.Abstractions;

public interface IAssetService
{
    Task<PagedResult<AssetDto>> GetPagedAsync(AssetQuery query, CancellationToken cancellationToken = default);

    Task<AssetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AssetDto> CreateAsync(CreateAssetRequest request, CancellationToken cancellationToken = default);

    Task<AssetDto> UpdateAsync(Guid id, UpdateAssetRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
