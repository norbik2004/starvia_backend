using MassTransit;
using Microsoft.Extensions.Logging;

namespace StarviaBackend.Modules.Accounts.Application.Users.Events.UserRegistered;

/// <summary>
/// Example consumer showing the publish/consume pattern end to end. Here it just logs;
/// in a real module this is where you would send a welcome email, seed defaults, etc.
/// </summary>
internal sealed class UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger)
    : IConsumer<UserRegisteredEvent>
{
    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        logger.LogInformation(
            "User registered: {UserId} ({Email}) at {RegisteredAt:o}",
            context.Message.UserId,
            context.Message.Email,
            context.Message.RegisteredAt);

        return Task.CompletedTask;
    }
}
