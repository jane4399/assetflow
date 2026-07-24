namespace AssetFlow.Application.Contracts.WorkOrders;

public record WorkOrderDto(
    Guid Id,
    string Title,
    string? Description,
    string Priority,
    string Status,
    Guid AssetId,
    string AssetName,
    Guid? AssignedTechnicianId,
    string? AssignedTechnicianName,
    DateTime? DueDate,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
