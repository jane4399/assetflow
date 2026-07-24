using AssetFlow.Application.Abstractions;
using AssetFlow.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AssetFlow.Application;

/// <summary>
/// Composition root for the Application layer. Registers the use-case services
/// and every FluentValidation validator discovered in this assembly.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<AssetFlow.Application.Validators.RegisterRequestValidator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISiteService, SiteService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IWorkOrderService, WorkOrderService>();

        return services;
    }
}
