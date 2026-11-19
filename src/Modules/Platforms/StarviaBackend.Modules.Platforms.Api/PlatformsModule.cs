using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Platforms.Application;
using StarviaBackend.Modules.Platforms.Core;
using StarviaBackend.Modules.Platforms.Infrastructure;

namespace StarviaBackend.Modules.Platforms.Api;

/// <summary>
/// The Platforms module's public entry point. The Bootstrapper calls
/// <see cref="RegisterPlatformsModule"/> to wire services and <see cref="RegisterAccountsConsumers"/>
/// to add its bus consumers.
/// </summary>
public static class PlatformsModule
{
    public static IServiceCollection RegisterPlatformsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCore();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services;
    }

    public static void RegisterAccountsConsumers(this IBusRegistrationConfigurator configurator)
    {
        // TODO:
        // configurator.AddConsumer<UserRegisteredConsumer>();
    }
}
