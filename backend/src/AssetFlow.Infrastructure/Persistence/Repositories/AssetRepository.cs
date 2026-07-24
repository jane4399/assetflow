using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Common;
using AssetFlow.Application.Contracts.Assets;
using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Persistence.Repositories;

public class AssetRepository : Repository<Asset>, IAssetRepository
{
    public AssetRepository(AssetFlowDbContext context)
        : base(context)
    {
    }

    public async Task<PagedResult<Asset>> SearchAsync(AssetQuery query, CancellationToken cancellationToken = default)
    {
        var q = Set.AsNoTracking().Include(a => a.Site).AsQueryable();

        if (query.Status is not null)
        {
            q = q.Where(a => a.Status == query.Status);
        }

        if (query.SiteId is not null)
        {
            q = q.Where(a => a.SiteId == query.SiteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(a => EF.Functions.Like(a.Name, $"%{term}%") || EF.Functions.Like(a.Tag, $"%{term}%"));
        }

        q = ApplySort(q, query);

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Asset>(items, total, query.Page, query.PageSize);
    }

    public async Task<Asset?> GetWithSiteAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Include(a => a.Site)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<bool> TagExistsAsync(string tag, Guid? excludeId = null, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(a => a.Tag == tag && (excludeId == null || a.Id != excludeId.Value), cancellationToken);

    public async Task<bool> HasWorkOrdersAsync(Guid assetId, CancellationToken cancellationToken = default) =>
        await Context.WorkOrders.AnyAsync(w => w.AssetId == assetId, cancellationToken);

    private static IQueryable<Asset> ApplySort(IQueryable<Asset> q, AssetQuery query)
    {
        var descending = query.IsDescending;
        // Status is intentionally not a sort key: it is stored as a string, so
        // ordering by it would be lexicographic rather than semantic. Use the
        // status filter instead.
        return (query.SortBy?.ToLowerInvariant()) switch
        {
            "name" => descending ? q.OrderByDescending(a => a.Name) : q.OrderBy(a => a.Name),
            "tag" => descending ? q.OrderByDescending(a => a.Tag) : q.OrderBy(a => a.Tag),
            "createdatutc" => descending ? q.OrderByDescending(a => a.CreatedAtUtc) : q.OrderBy(a => a.CreatedAtUtc),
            _ => q.OrderBy(a => a.Name)
        };
    }
}
