using AssetFlow.Application.Common;
using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Contracts.WorkOrders;

/// <summary>Query-string filter for listing work orders. Bound with <c>[FromQuery]</c>.</summary>
public class WorkOrderQuery : PaginationQuery
{
    public WorkOrderStatus? Status { get; set; }

    public WorkOrderPriority? Priority { get; set; }

    public Guid? AssetId { get; set; }

    public Guid? AssignedTechnicianId { get; set; }

    /// <summary>Free-text match against the work-order title (case-insensitive contains).</summary>
    public string? Search { get; set; }
}
