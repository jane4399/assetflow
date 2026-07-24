using AssetFlow.Domain.Common;

namespace AssetFlow.Domain.Entities;

/// <summary>
/// A serviceable piece of equipment (pump, valve, turbine) that lives at a
/// <see cref="Site"/> and is the subject of <see cref="WorkOrder"/>s.
/// </summary>
public class Asset : AuditableEntity
{
    public required string Name { get; set; }

    /// <summary>Asset tag / nameplate identifier, unique across the system.</summary>
    public required string Tag { get; set; }

    public AssetStatus Status { get; set; } = AssetStatus.Operational;

    public Guid SiteId { get; set; }

    public Site? Site { get; set; }

    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
