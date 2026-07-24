using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Common;
using AssetFlow.Application.Common.Exceptions;
using AssetFlow.Application.Contracts.Assets;
using AssetFlow.Application.Mapping;
using AssetFlow.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AssetFlow.Application.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _assets;
    private readonly ISiteRepository _sites;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateAssetRequest> _createValidator;
    private readonly IValidator<UpdateAssetRequest> _updateValidator;
    private readonly ILogger<AssetService> _logger;

    public AssetService(
        IAssetRepository assets,
        ISiteRepository sites,
        IUnitOfWork unitOfWork,
        IValidator<CreateAssetRequest> createValidator,
        IValidator<UpdateAssetRequest> updateValidator,
        ILogger<AssetService> logger)
    {
        _assets = assets;
        _sites = sites;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<AssetDto>> GetPagedAsync(AssetQuery query, CancellationToken cancellationToken = default)
    {
        var page = await _assets.SearchAsync(query, cancellationToken);
        var dtos = page.Items.Select(a => a.ToDto()).ToList();
        return new PagedResult<AssetDto>(dtos, page.TotalCount, page.Page, page.PageSize);
    }

    public async Task<AssetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var asset = await _assets.GetWithSiteAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Asset), id);
        return asset.ToDto();
    }

    public async Task<AssetDto> CreateAsync(CreateAssetRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var site = await _sites.GetByIdAsync(request.SiteId, cancellationToken)
            ?? throw new NotFoundException(nameof(Site), request.SiteId);

        var tag = request.Tag.Trim();
        if (await _assets.TagExistsAsync(tag, null, cancellationToken))
        {
            throw new ConflictException($"An asset with tag '{tag}' already exists.");
        }

        var asset = new Asset
        {
            Name = request.Name.Trim(),
            Tag = tag,
            Status = request.Status,
            SiteId = site.Id,
            Site = site
        };

        await _assets.AddAsync(asset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created asset {AssetId} ({Tag}) at site {SiteId}", asset.Id, asset.Tag, site.Id);
        return asset.ToDto();
    }

    public async Task<AssetDto> UpdateAsync(Guid id, UpdateAssetRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        // Fetch tracked (no navigation graph) so change tracking persists just the
        // modified scalars and never cascades a spurious UPDATE to the related site.
        var asset = await _assets.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Asset), id);

        if (asset.SiteId != request.SiteId)
        {
            if (await _sites.GetByIdAsync(request.SiteId, cancellationToken) is null)
            {
                throw new NotFoundException(nameof(Site), request.SiteId);
            }

            asset.SiteId = request.SiteId;
        }

        var tag = request.Tag.Trim();
        if (!string.Equals(tag, asset.Tag, StringComparison.OrdinalIgnoreCase)
            && await _assets.TagExistsAsync(tag, id, cancellationToken))
        {
            throw new ConflictException($"An asset with tag '{tag}' already exists.");
        }

        asset.Name = request.Name.Trim();
        asset.Tag = tag;
        asset.Status = request.Status;

        _assets.Update(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Re-read with the owning site for an accurate SiteName in the response.
        var updated = await _assets.GetWithSiteAsync(id, cancellationToken);
        return (updated ?? asset).ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var asset = await _assets.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Asset), id);

        if (await _assets.HasWorkOrdersAsync(id, cancellationToken))
        {
            throw new ConflictException("Cannot delete an asset that still has work orders.");
        }

        _assets.Remove(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted asset {AssetId}", id);
    }
}
