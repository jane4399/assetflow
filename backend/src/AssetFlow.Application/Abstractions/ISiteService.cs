using AssetFlow.Application.Common;
using AssetFlow.Application.Contracts.Sites;

namespace AssetFlow.Application.Abstractions;

public interface ISiteService
{
    Task<PagedResult<SiteDto>> GetPagedAsync(SiteQuery query, CancellationToken cancellationToken = default);

    Task<SiteDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SiteDto> CreateAsync(CreateSiteRequest request, CancellationToken cancellationToken = default);

    Task<SiteDto> UpdateAsync(Guid id, UpdateSiteRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
