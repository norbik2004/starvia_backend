using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;
using StarviaBackend.Shared.Infrastructure.Cqrs;
using StarviaBackend.Shared.Infrastructure.Postgres;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Initializers;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Users.Repositories;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Platforms.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Platforms.Tests.Integration")]
namespace StarviaBackend.Modules.Platforms.Infrastructure;

internal static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddPostgres<PlatformsWriteDbContext>();
        services.AddPostgres<PlatformsReadDbContext>();

        services.AddScoped<IPlatformRepository, PlatformRepository>();
        services.AddHostedService<PlatformsDataInitializer>();

        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
