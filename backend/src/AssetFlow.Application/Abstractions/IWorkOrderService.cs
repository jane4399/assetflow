using AssetFlow.Application.Common;
using AssetFlow.Application.Contracts.WorkOrders;

namespace AssetFlow.Application.Abstractions;

public interface IWorkOrderService
{
    Task<PagedResult<WorkOrderDto>> GetPagedAsync(WorkOrderQuery query, CancellationToken cancellationToken = default);

    Task<WorkOrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<WorkOrderDto> CreateAsync(CreateWorkOrderRequest request, CancellationToken cancellationToken = default);

    Task<WorkOrderDto> UpdateAsync(Guid id, UpdateWorkOrderRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
