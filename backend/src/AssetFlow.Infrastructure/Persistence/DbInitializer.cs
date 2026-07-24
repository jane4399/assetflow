using AssetFlow.Application.Abstractions;
using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Persistence;

/// <summary>
/// Idempotent development seed. Guarantees a known admin/technician login and a
/// small slice of realistic asset data so the API and Angular app are usable the
/// moment the database is created.
/// </summary>
public static class DbInitializer
{
    public const string AdminEmail = "admin@assetflow.io";
    public const string AdminPassword = "Admin123!";
    public const string TechnicianEmail = "tech@assetflow.io";
    public const string TechnicianPassword = "Tech123!";

    public static async Task SeedAsync(
        AssetFlowDbContext context,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        if (!await context.Users.AnyAsync(u => u.Email == AdminEmail, cancellationToken))
        {
            context.Users.Add(new User
            {
                Email = AdminEmail,
                FullName = "AssetFlow Administrator",
                PasswordHash = passwordHasher.Hash(AdminPassword),
                Role = UserRole.Admin
            });
        }

        var technician = await context.Users
            .FirstOrDefaultAsync(u => u.Email == TechnicianEmail, cancellationToken);
        if (technician is null)
        {
            technician = new User
            {
                Email = TechnicianEmail,
                FullName = "Sam Technician",
                PasswordHash = passwordHasher.Hash(TechnicianPassword),
                Role = UserRole.Technician
            };
            context.Users.Add(technician);
        }

        if (!await context.Sites.AnyAsync(cancellationToken))
        {
            var site = new Site
            {
                Name = "Houston Ship Channel Terminal",
                Code = "HOU-01",
                Location = "Houston, TX"
            };

            var feedPump = new Asset
            {
                Name = "Feed Pump A",
                Tag = "PMP-1001",
                Status = AssetStatus.Operational,
                Site = site
            };

            var compressor = new Asset
            {
                Name = "Gas Compressor 2",
                Tag = "CMP-2002",
                Status = AssetStatus.Maintenance,
                Site = site
            };

            context.Sites.Add(site);
            context.Assets.AddRange(feedPump, compressor);

            context.WorkOrders.AddRange(
                new WorkOrder
                {
                    Title = "Replace mechanical seal",
                    Description = "Seal weeping on the outboard side; schedule replacement during next window.",
                    Priority = WorkOrderPriority.High,
                    Status = WorkOrderStatus.Open,
                    Asset = feedPump,
                    AssignedTechnician = technician,
                    DueDate = DateTime.UtcNow.AddDays(3)
                },
                new WorkOrder
                {
                    Title = "Quarterly vibration analysis",
                    Description = "Collect vibration signatures and compare against baseline.",
                    Priority = WorkOrderPriority.Medium,
                    Status = WorkOrderStatus.InProgress,
                    Asset = compressor,
                    DueDate = DateTime.UtcNow.AddDays(7)
                });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
