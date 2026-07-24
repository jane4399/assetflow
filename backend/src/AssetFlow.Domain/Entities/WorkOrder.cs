using AssetFlow.Domain.Common;

namespace AssetFlow.Domain.Entities;

/// <summary>
/// A unit of maintenance work raised against an <see cref="Asset"/> and
/// optionally assigned to a technician (<see cref="User"/>).
/// </summary>
public class WorkOrder : AuditableEntity
{
    public required string Title { get; set; }

    public string? Description { get; set; }

    public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;

    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Open;

    public Guid AssetId { get; set; }

    public Asset? Asset { get; set; }

    public Guid? AssignedTechnicianId { get; set; }

    public User? AssignedTechnician { get; set; }

    public DateTime? DueDate { get; set; }
}
