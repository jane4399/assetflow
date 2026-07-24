namespace AssetFlow.Domain.Entities;

/// <summary>Lifecycle state of a work order.</summary>
public enum WorkOrderStatus
{
    Open = 1,
    InProgress = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5
}
