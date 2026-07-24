using AssetFlow.Domain.Common;

namespace AssetFlow.Domain.Entities;

/// <summary>
/// A physical location (plant, terminal, compressor station) that owns assets.
/// </summary>
public class Site : AuditableEntity
{
    public required string Name { get; set; }

    /// <summary>Short human-readable code, unique across the system (e.g. "HOU-01").</summary>
    public required string Code { get; set; }

    public string? Location { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
