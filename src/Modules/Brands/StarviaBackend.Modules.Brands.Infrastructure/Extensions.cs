using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Repositories;
using StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Repositories;
using StarviaBackend.Modules.Brands.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Brands.Infrastructure.EF.Initializers;
using StarviaBackend.Shared.Infrastructure.Cqrs;
using StarviaBackend.Shared.Infrastructure.Postgres;

[assembly: InternalsVisibleTo("StarviaBackend.Modules.Brands.Api")]
[assembly: InternalsVisibleTo("StarviaBackend.Modules.Brands.Tests.Integration")]
namespace StarviaBackend.Modules.Brands.Infrastructure;

internal static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddPostgres<BrandsWriteDbContext>();
        services.AddPostgres<BrandsReadDbContext>();

        services.AddScoped<IBrandProfileRepository, BrandProfileRepository>();

        services.AddHostedService<BrandsDataInitializer>();

        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
