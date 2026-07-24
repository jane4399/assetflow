namespace AssetFlow.Domain.Entities;

/// <summary>
/// Operational state of a physical asset. Persisted as a string (see
/// <c>AssetConfiguration</c>) so the database stays readable and stable if new
/// members are inserted.
/// </summary>
public enum AssetStatus
{
    Operational = 1,
    Maintenance = 2,
    Offline = 3,
    Decommissioned = 4
}
