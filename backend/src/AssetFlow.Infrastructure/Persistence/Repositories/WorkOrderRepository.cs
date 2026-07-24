using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Common;
using AssetFlow.Application.Contracts.WorkOrders;
using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Persistence.Repositories;

public class WorkOrderRepository : Repository<WorkOrder>, IWorkOrderRepository
{
    public WorkOrderRepository(AssetFlowDbContext context)
        : base(context)
    {
    }

    public async Task<PagedResult<WorkOrder>> SearchAsync(WorkOrderQuery query, CancellationToken cancellationToken = default)
    {
        var q = Set.AsNoTracking()
            .Include(w => w.Asset)
            .Include(w => w.AssignedTechnician)
            .AsQueryable();

        if (query.Status is not null)
        {
            q = q.Where(w => w.Status == query.Status.Value);
        }

        if (query.Priority is not null)
        {
            q = q.Where(w => w.Priority == query.Priority.Value);
        }

        if (query.AssetId is not null)
        {
            q = q.Where(w => w.AssetId == query.AssetId.Value);
        }

        if (query.AssignedTechnicianId is not null)
        {
            q = q.Where(w => w.AssignedTechnicianId == query.AssignedTechnicianId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(w => EF.Functions.Like(w.Title, $"%{term}%"));
        }

        q = ApplySort(q, query);

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<WorkOrder>(items, total, query.Page, query.PageSize);
    }

    public async Task<WorkOrder?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Include(w => w.Asset)
            .Include(w => w.AssignedTechnician)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    private static IQueryable<WorkOrder> ApplySort(IQueryable<WorkOrder> q, WorkOrderQuery query)
    {
        var descending = query.IsDescending;

        // Priority and Status are stored as strings and so are filtered rather
        // than sorted; the sortable keys below all order naturally in SQL.
        return (query.SortBy?.ToLowerInvariant()) switch
        {
            "title" => descending ? q.OrderByDescending(w => w.Title) : q.OrderBy(w => w.Title),
            "duedate" => descending ? q.OrderByDescending(w => w.DueDate) : q.OrderBy(w => w.DueDate),
            "createdatutc" => descending ? q.OrderByDescending(w => w.CreatedAtUtc) : q.OrderBy(w => w.CreatedAtUtc),
            _ => q.OrderByDescending(w => w.CreatedAtUtc)
        };
    }
}
