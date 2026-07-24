using AssetFlow.Application.Common;
using AssetFlow.Application.Contracts.WorkOrders;
using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Abstractions;

public interface IWorkOrderRepository : IRepository<WorkOrder>
{
    /// <summary>Search with <see cref="Asset"/> and assigned technician eagerly loaded.</summary>
    Task<PagedResult<WorkOrder>> SearchAsync(WorkOrderQuery query, CancellationToken cancellationToken = default);

    /// <summary>Fetch a single work order with related entities loaded for projection.</summary>
    Task<WorkOrder?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
