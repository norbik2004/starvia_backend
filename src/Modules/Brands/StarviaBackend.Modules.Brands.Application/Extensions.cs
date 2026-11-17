using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Access;
using StarviaBackend.Shared.Infrastructure.Cqrs;

[assembly: InternalsVisibleTo("StarviaBackend.Modules.Brands.Infrastructure")]
[assembly: InternalsVisibleTo("StarviaBackend.Modules.Brands.Api")]
[assembly: InternalsVisibleTo("StarviaBackend.Modules.Brands.Tests.Integration")]
namespace StarviaBackend.Modules.Brands.Application;

internal static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        services.AddScoped<IBrandAccessService, BrandAccessService>();
        return services;
    }
}
