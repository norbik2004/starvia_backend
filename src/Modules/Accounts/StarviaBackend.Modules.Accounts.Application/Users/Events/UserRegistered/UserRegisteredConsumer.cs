using MassTransit;
using Microsoft.Extensions.Logging;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ConfirmEmail;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.GenerateEmailConfirmationToken;
using StarviaBackend.Shared.Abstractions.App;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Accounts.Application.Users.Events.UserRegistered;

/// <summary>
/// After registration: generate a confirmation token and queue a confirm-account email via MassTransit.
/// </summary>
internal sealed class UserRegisteredConsumer(
    ILogger<UserRegisteredConsumer> logger,
    IDispatcher dispatcher,
    IPublishEndpoint publishEndpoint,
    IAppUrls appUrls)
    : IConsumer<UserRegisteredEvent>
{
    private const string ConfirmAccountEmailType = "ConfirmAccountEmail";

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        logger.LogInformation(
            "User registered: {UserId} ({Email}) at {RegisteredAt:o}",
            context.Message.UserId,
            context.Message.Email,
            context.Message.RegisteredAt);

        var code = await dispatcher.SendAsync<GenerateEmailConfirmationTokenCommand, GenerateEmailConfirmationTokenResult>(
            new GenerateEmailConfirmationTokenCommand(context.Message.UserId),
            context.CancellationToken);

        var confirmationLink = EmailConfirmationLink.Build(
            appUrls.FrontendBaseUrl,
            context.Message.UserId,
            code.Token);

        await publishEndpoint.Publish(
            new SendEmailRequestedEvent(
                context.Message.UserId,
                context.Message.Email,
                ConfirmAccountEmailType,
                Code: code.Token,
                UserName: context.Message.Email,
                Link: confirmationLink),
            context.CancellationToken);
    }
}
