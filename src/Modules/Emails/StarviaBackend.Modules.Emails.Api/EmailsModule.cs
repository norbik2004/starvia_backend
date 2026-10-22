using FluentValidation;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Emails.Application;
using StarviaBackend.Modules.Emails.Application.Emails.Events.SendEmailRequested;
using StarviaBackend.Modules.Emails.Core;
using StarviaBackend.Modules.Emails.Core.Emails.Exceptions;
using StarviaBackend.Modules.Emails.Infrastructure;
using StarviaBackend.Shared.Abstractions.Accounts;

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
        configurator.AddConsumer<SendEmailRequestedConsumer>((context, cfg) =>
        {
            // Fetching the account token goes over the bus and can fail transiently (timeout,
            // responder briefly unavailable), as can SMTP. A rejected token or an invalid request
            // can never succeed, so those fail straight to the error queue.
            cfg.UseMessageRetry(r =>
            {
                r.Interval(3, TimeSpan.FromSeconds(60));
                r.Ignore<AccountTokenRejectedException>();
                r.Ignore<ValidationException>();
            });
        });

        // Emails never references Accounts; it asks for tokens through the contracts in
        // Shared.Abstractions.Accounts, answered by consumers registered in the Accounts module.
        configurator.AddRequestClient<GenerateEmailConfirmationTokenRequest>(RequestTimeout.After(s: 30));
        configurator.AddRequestClient<GeneratePasswordResetTokenRequest>(RequestTimeout.After(s: 30));
    }
}
