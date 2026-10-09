using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Media.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Media.Infrastructure.EF.Initializers;
using StarviaBackend.Modules.Media.Core.Media.Repositories;
using StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Repositories;
using StarviaBackend.Modules.Media.Infrastructure.Storage.RustFs;
using StarviaBackend.Shared.Infrastructure.Cqrs;
using StarviaBackend.Shared.Infrastructure.Postgres;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Media.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Media.Tests.Integration")]
namespace StarviaBackend.Modules.Media.Infrastructure;

internal static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddPostgres<MediaWriteDbContext>();
        services.AddPostgres<MediaReadDbContext>();

        services.AddScoped<IMediaFileRepository, MediaFileRepository>();

        services.AddRustFs();

        services.AddHostedService<MediaDataInitializer>();

        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
