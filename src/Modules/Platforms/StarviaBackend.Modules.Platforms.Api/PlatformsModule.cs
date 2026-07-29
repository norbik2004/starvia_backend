using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Platforms.Application;
using StarviaBackend.Modules.Platforms.Core;
using StarviaBackend.Modules.Platforms.Infrastructure;

namespace StarviaBackend.Modules.Platforms.Api;

/// <summary>
/// The Accounts module's public entry point. The Bootstrapper calls
/// <see cref="RegisterAccountsModule"/> to wire services and <see cref="RegisterAccountsConsumers"/>
/// to add its bus consumers.
/// </summary>
public static class PlatformsModule
{
    public static IServiceCollection RegisterPlatformsModule(this IServiceCollection services)
    {
        services.AddCore();
        services.AddApplication();
        services.AddInfrastructure();
        return services;
    }

    public static void RegisterAccountsConsumers(this IBusRegistrationConfigurator configurator)
    {
        // TODO:
        // configurator.AddConsumer<UserRegisteredConsumer>();
    }
}
