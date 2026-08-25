using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Accounts.Application;
using StarviaBackend.Modules.Accounts.Application.Users.Events.UserRegistered;
using StarviaBackend.Modules.Accounts.Core;
using StarviaBackend.Modules.Accounts.Infrastructure;

namespace StarviaBackend.Modules.Accounts.Api;

/// <summary>
/// The Accounts module's public entry point. The Bootstrapper calls
/// <see cref="RegisterAccountsModule"/> to wire services and <see cref="RegisterAccountsConsumers"/>
/// to add its bus consumers.
/// </summary>
public static class AccountsModule
{
    public static IServiceCollection RegisterAccountsModule(this IServiceCollection services)
    {
        services.AddCore();
        services.AddApplication();
        services.AddInfrastructure();
        return services;
    }

    public static void RegisterAccountsConsumers(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<UserRegisteredConsumer>((context, cfg) =>
        {
            cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(60)));
        });
    }
}
