using AssetFlow.Application.Abstractions;
using AssetFlow.Infrastructure.Persistence;
using AssetFlow.Infrastructure.Persistence.Repositories;
using AssetFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AssetFlow.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer: the EF Core DbContext (with a
/// runtime-selectable database provider), the concrete repositories, and the
/// security primitives.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var provider = configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        var migrationsAssembly = typeof(AssetFlowDbContext).Assembly.FullName;

        services.AddDbContext<AssetFlowDbContext>(options =>
        {
            if (string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(migrationsAssembly));
            }
            else
            {
                options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
            }
        });

        // Expose the DbContext as the unit of work so services commit without
        // referencing Entity Framework directly.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AssetFlowDbContext>());

        services.AddScoped<ISiteRepository, SiteRepository>();
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
