using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetFlow.Infrastructure.Persistence.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(2000);

        builder.Property(w => w.Priority)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(w => w.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.HasIndex(w => w.Status);
        builder.HasIndex(w => w.Priority);
        builder.HasIndex(w => w.AssetId);
        builder.HasIndex(w => w.AssignedTechnicianId);

        // Asset -> WorkOrder relationship is configured on AssetConfiguration.
        // Here we wire the optional technician assignment.
        builder.HasOne(w => w.AssignedTechnician)
            .WithMany(u => u.AssignedWorkOrders)
            .HasForeignKey(w => w.AssignedTechnicianId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
