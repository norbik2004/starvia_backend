using StarviaBackend.Shared.Infrastructure.Messaging;

namespace StarviaBackend.Bootstrapper.Bootstrap;

internal static class Messaging
{
    public static IServiceCollection AddAppMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddMessaging(configuration, configurator =>
        {
            //configurator.RegisterAccountsConsumers();
        });
    }
}