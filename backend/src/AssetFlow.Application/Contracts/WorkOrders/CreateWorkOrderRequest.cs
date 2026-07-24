using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Contracts.WorkOrders;

public record CreateWorkOrderRequest(
    string Title,
    string? Description,
    WorkOrderPriority Priority,
    Guid AssetId,
    Guid? AssignedTechnicianId,
    DateTime? DueDate);
