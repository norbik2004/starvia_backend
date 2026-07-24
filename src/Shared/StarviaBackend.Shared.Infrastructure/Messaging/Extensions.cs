using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StarviaBackend.Shared.Infrastructure.Messaging;

public static class Extensions
{
    /// <summary>
    /// Wires MassTransit with the in-memory transport. Each module contributes its consumers
    /// through <paramref name="registerConsumers"/> (e.g. <c>x =&gt; x.RegisterAccountsConsumers()</c>).
    /// Swap <c>UsingInMemory</c> for a real transport (RabbitMq/Azure/SQS) without touching modules.
    /// </summary>
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator> registerConsumers)
    {
        services.AddMassTransit(x =>
        {
            registerConsumers(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMq"));
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
