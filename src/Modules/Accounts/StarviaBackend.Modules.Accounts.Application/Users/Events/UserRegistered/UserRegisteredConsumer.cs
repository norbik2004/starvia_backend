using MassTransit;
using Microsoft.Extensions.Logging;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.GenerateEmailConfirmationToken;
using StarviaBackend.Shared.Abstractions.Dispatchers;

namespace StarviaBackend.Modules.Accounts.Application.Users.Events.UserRegistered;

/// <summary>
/// Example consumer showing the publish/consume pattern end to end. Here it just logs;
/// in a real module this is where you would send a welcome email, seed defaults, etc.
/// </summary>
internal sealed class UserRegisteredConsumer(
    ILogger<UserRegisteredConsumer> logger,
    IDispatcher dispatcher)
    : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        logger.LogInformation(
            "User registered: {UserId} ({Email}) at {RegisteredAt:o}",
            context.Message.UserId,
            context.Message.Email,
            context.Message.RegisteredAt);

        var code = await dispatcher.SendAsync<GenerateEmailConfirmationTokenCommand, GenerateEmailConfirmationTokenResult>
            (new GenerateEmailConfirmationTokenCommand(context.Message.UserId));

        // TODO: sending email with Code
    }
}
