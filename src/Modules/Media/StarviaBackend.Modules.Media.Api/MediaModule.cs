using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Media.Core;
using StarviaBackend.Modules.Media.Infrastructure;
using StarviaBackend.Modules.Media.Application;

namespace StarviaBackend.Modules.Media.Api;

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
