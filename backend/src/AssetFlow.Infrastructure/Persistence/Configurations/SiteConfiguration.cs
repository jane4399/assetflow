using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetFlow.Infrastructure.Persistence.Configurations;

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.ToTable("Sites");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Location)
            .HasMaxLength(300);

        builder.HasIndex(s => s.Code).IsUnique();

        builder.HasMany(s => s.Assets)
            .WithOne(a => a.Site!)
            .HasForeignKey(a => a.SiteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
