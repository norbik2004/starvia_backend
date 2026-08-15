using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Emails.Application;
using StarviaBackend.Modules.Emails.Application.Emails.Events.SendEmailRequested;
using StarviaBackend.Modules.Emails.Core;
using StarviaBackend.Modules.Emails.Infrastructure;

namespace StarviaBackend.Modules.Emails.Api;

/// <summary>
/// The Emails module's public entry point. The Bootstrapper calls
/// <see cref="RegisterEmailsModule"/> to wire services and <see cref="RegisterEmailsConsumers"/>
/// to add its bus consumers.
/// </summary>
public static class EmailsModule
{
    public static IServiceCollection RegisterEmailsModule(this IServiceCollection services)
    {
        services.AddCore();
        services.AddApplication();
        services.AddInfrastructure();
        return services;
    }

    public static void RegisterEmailsConsumers(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<SendEmailRequestedConsumer>();
    }
}
