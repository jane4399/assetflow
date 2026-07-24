using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Common;
using AssetFlow.Application.Common.Exceptions;
using AssetFlow.Application.Contracts.Sites;
using AssetFlow.Application.Mapping;
using AssetFlow.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AssetFlow.Application.Services;

public class SiteService : ISiteService
{
    private readonly ISiteRepository _sites;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateSiteRequest> _createValidator;
    private readonly IValidator<UpdateSiteRequest> _updateValidator;
    private readonly ILogger<SiteService> _logger;

    public SiteService(
        ISiteRepository sites,
        IUnitOfWork unitOfWork,
        IValidator<CreateSiteRequest> createValidator,
        IValidator<UpdateSiteRequest> updateValidator,
        ILogger<SiteService> logger)
    {
        _sites = sites;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<SiteDto>> GetPagedAsync(SiteQuery query, CancellationToken cancellationToken = default)
    {
        var page = await _sites.SearchAsync(query, cancellationToken);
        var counts = await _sites.GetAssetCountsAsync(
            page.Items.Select(s => s.Id).ToArray(),
            cancellationToken);

        var dtos = page.Items
            .Select(s => s.ToDto(counts.TryGetValue(s.Id, out var count) ? count : 0))
            .ToList();

        return new PagedResult<SiteDto>(dtos, page.TotalCount, page.Page, page.PageSize);
    }

    public async Task<SiteDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var site = await _sites.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Site), id);

        var counts = await _sites.GetAssetCountsAsync(new[] { id }, cancellationToken);
        return site.ToDto(counts.TryGetValue(id, out var count) ? count : 0);
    }

    public async Task<SiteDto> CreateAsync(CreateSiteRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var code = request.Code.Trim();
        if (await _sites.CodeExistsAsync(code, null, cancellationToken))
        {
            throw new ConflictException($"A site with code '{code}' already exists.");
        }

        var site = new Site
        {
            Name = request.Name.Trim(),
            Code = code,
            Location = request.Location?.Trim()
        };

        await _sites.AddAsync(site, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created site {SiteId} ({Code})", site.Id, site.Code);
        return site.ToDto(0);
    }

    public async Task<SiteDto> UpdateAsync(Guid id, UpdateSiteRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var site = await _sites.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Site), id);

        var code = request.Code.Trim();
        if (!string.Equals(code, site.Code, StringComparison.OrdinalIgnoreCase)
            && await _sites.CodeExistsAsync(code, id, cancellationToken))
        {
            throw new ConflictException($"A site with code '{code}' already exists.");
        }

        site.Name = request.Name.Trim();
        site.Code = code;
        site.Location = request.Location?.Trim();

        _sites.Update(site);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var counts = await _sites.GetAssetCountsAsync(new[] { id }, cancellationToken);
        return site.ToDto(counts.TryGetValue(id, out var count) ? count : 0);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var site = await _sites.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Site), id);

        if (await _sites.HasAssetsAsync(id, cancellationToken))
        {
            throw new ConflictException("Cannot delete a site that still owns assets. Reassign or remove them first.");
        }

        _sites.Remove(site);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted site {SiteId}", id);
    }
}
