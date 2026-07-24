using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Common;
using AssetFlow.Application.Contracts.Sites;
using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Persistence.Repositories;

public class SiteRepository : Repository<Site>, ISiteRepository
{
    public SiteRepository(AssetFlowDbContext context)
        : base(context)
    {
    }

    public async Task<PagedResult<Site>> SearchAsync(SiteQuery query, CancellationToken cancellationToken = default)
    {
        var q = Set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(s => EF.Functions.Like(s.Name, $"%{term}%") || EF.Functions.Like(s.Code, $"%{term}%"));
        }

        q = ApplySort(q, query);

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Site>(items, total, query.Page, query.PageSize);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(s => s.Code == code && (excludeId == null || s.Id != excludeId.Value), cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, int>> GetAssetCountsAsync(
        IReadOnlyCollection<Guid> siteIds,
        CancellationToken cancellationToken = default)
    {
        if (siteIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        return await Context.Assets
            .Where(a => siteIds.Contains(a.SiteId))
            .GroupBy(a => a.SiteId)
            .Select(g => new { SiteId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SiteId, x => x.Count, cancellationToken);
    }

    public async Task<bool> HasAssetsAsync(Guid siteId, CancellationToken cancellationToken = default) =>
        await Context.Assets.AnyAsync(a => a.SiteId == siteId, cancellationToken);

    private static IQueryable<Site> ApplySort(IQueryable<Site> q, SiteQuery query)
    {
        var descending = query.IsDescending;
        return (query.SortBy?.ToLowerInvariant()) switch
        {
            "name" => descending ? q.OrderByDescending(s => s.Name) : q.OrderBy(s => s.Name),
            "code" => descending ? q.OrderByDescending(s => s.Code) : q.OrderBy(s => s.Code),
            "createdatutc" => descending ? q.OrderByDescending(s => s.CreatedAtUtc) : q.OrderBy(s => s.CreatedAtUtc),
            _ => q.OrderBy(s => s.Name)
        };
    }
}
