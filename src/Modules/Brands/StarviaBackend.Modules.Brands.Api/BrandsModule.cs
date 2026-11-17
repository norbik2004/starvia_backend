using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Brands.Application;
using StarviaBackend.Modules.Brands.Core;
using StarviaBackend.Modules.Brands.Infrastructure;

namespace StarviaBackend.Modules.Brands.Api;

public static class BrandsModule
{
    public static IServiceCollection RegisterBrandsModule(this IServiceCollection services)
    {
        services.AddCore();
        services.AddApplication();
        services.AddInfrastructure();
        return services;
    }
}
