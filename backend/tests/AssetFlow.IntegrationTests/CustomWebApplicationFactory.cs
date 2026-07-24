using AssetFlow.Application.Abstractions;
using AssetFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace AssetFlow.IntegrationTests;

/// <summary>
/// Boots the real API in-process but swaps SQL Server for an in-memory EF Core
/// provider (shared root so the seed and the request pipeline see one store).
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot DatabaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Strip every EF registration tied to AssetFlowDbContext — including the
            // EF Core 8 IDbContextOptionsConfiguration<T> that still carries the SQL
            // Server provider — so only the in-memory provider remains.
            var toRemove = services.Where(descriptor =>
                    descriptor.ServiceType == typeof(DbContextOptions<AssetFlowDbContext>)
                    || descriptor.ServiceType == typeof(AssetFlowDbContext)
                    || (descriptor.ServiceType.IsGenericType
                        && descriptor.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration", StringComparison.Ordinal)
                        && descriptor.ServiceType.GenericTypeArguments.Contains(typeof(AssetFlowDbContext))))
                .ToList();

            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AssetFlowDbContext>(options =>
                options.UseInMemoryDatabase("AssetFlowIntegrationTests", DatabaseRoot));
        });
    }

    /// <summary>Idempotently creates and seeds the in-memory store via the app's own provider.</summary>
    public async Task SeedAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AssetFlowDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await dbContext.Database.EnsureCreatedAsync();
        await DbInitializer.SeedAsync(dbContext, hasher);
    }
}
