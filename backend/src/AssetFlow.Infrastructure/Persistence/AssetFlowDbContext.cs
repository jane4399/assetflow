using AssetFlow.Application.Abstractions;
using AssetFlow.Domain.Common;
using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Persistence;

/// <summary>
/// The EF Core unit of work. Owns the entity sets, applies the Fluent
/// configurations, and stamps audit timestamps centrally on save. Implements
/// <see cref="IUnitOfWork"/> so the Application layer can commit without seeing
/// Entity Framework.
/// </summary>
public class AssetFlowDbContext : DbContext, IUnitOfWork
{
    public AssetFlowDbContext(DbContextOptions<AssetFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Site> Sites => Set<Site>();

    public DbSet<Asset> Assets => Set<Asset>();

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssetFlowDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    private void ApplyAuditTimestamps()
    {
        var nowUtc = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = nowUtc;
                    entry.Entity.UpdatedAtUtc = nowUtc;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = nowUtc;
                    // Protect the immutable creation timestamp from client tampering.
                    entry.Property(nameof(AuditableEntity.CreatedAtUtc)).IsModified = false;
                    break;
            }
        }
    }
}
