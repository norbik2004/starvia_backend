using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Ai.Application;
using StarviaBackend.Modules.Ai.Core;
using StarviaBackend.Modules.Ai.Infrastructure;

namespace StarviaBackend.Modules.Ai.Api;

public static class AiModule
{
    public static IServiceCollection RegisterAiModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCore();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services;
    }
}
