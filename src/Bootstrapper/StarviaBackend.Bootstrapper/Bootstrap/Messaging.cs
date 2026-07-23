using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Accounts.Api;
using StarviaBackend.Shared.Infrastructure.Messaging;

namespace ModularMonolith.Bootstrapper.Bootstrap;

internal static class Messaging
{
    public static IServiceCollection AddAppMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddMessaging(configuration, configurator =>
        {
            configurator.RegisterAccountsConsumers();
        });
    }
}
