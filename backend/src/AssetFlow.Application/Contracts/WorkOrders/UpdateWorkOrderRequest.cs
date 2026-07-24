using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Contracts.WorkOrders;

public record UpdateWorkOrderRequest(
    string Title,
    string? Description,
    WorkOrderPriority Priority,
    WorkOrderStatus Status,
    Guid? AssignedTechnicianId,
    DateTime? DueDate);
