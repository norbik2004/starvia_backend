using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Media.Application;
using StarviaBackend.Modules.Media.Core;
using StarviaBackend.Modules.Media.Infrastructure;

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
