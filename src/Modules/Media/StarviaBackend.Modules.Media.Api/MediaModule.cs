using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.MediaFiles.Core;
using StarviaBackend.Modules.MediaFiles.Infrastructure;
using StarviaBackend.Modules.MediaFiles.Application;

namespace StarviaBackend.Modules.MediaFiles.Api;

public static class MediaModule
{
    public static IServiceCollection RegisterMediaModule(this IServiceCollection services)
    {
        services.AddCore();
        services.AddApplication();
        services.AddInfrastructure();
        return services;
    }
}
