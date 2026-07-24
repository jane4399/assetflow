using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetFlow.Infrastructure.Persistence.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Tag)
            .IsRequired()
            .HasMaxLength(100);

        // Persist the enum as its name so the column stays human-readable and is
        // not broken by re-ordering the enum members.
        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.HasIndex(a => a.Tag).IsUnique();
        builder.HasIndex(a => a.SiteId);

        builder.HasMany(a => a.WorkOrders)
            .WithOne(w => w.Asset!)
            .HasForeignKey(w => w.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
