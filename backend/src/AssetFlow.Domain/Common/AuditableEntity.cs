namespace AssetFlow.Domain.Common;

/// <summary>
/// Base type for persisted aggregate roots. Carries the surrogate key and
/// UTC audit timestamps that every table in AssetFlow shares. Timestamps are
/// maintained centrally by <c>AssetFlowDbContext.SaveChangesAsync</c> so that
/// no service has to remember to set them.
/// </summary>
public abstract class AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
