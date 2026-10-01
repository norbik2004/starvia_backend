using MassTransit;
using Microsoft.Extensions.Logging;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Accounts.Application.Users.Events.UserRegistered;

/// <summary>
/// After registration: queue a confirm-account email via MassTransit.
/// The email module generates the confirmation token and builds the link.
/// </summary>
internal sealed class UserRegisteredConsumer(
    ILogger<UserRegisteredConsumer> logger,
    IPublishEndpoint publishEndpoint)
    : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        logger.LogInformation(
            "User registered: {UserId} ({Email}) at {RegisteredAt:o}",
            context.Message.UserId,
            context.Message.Email,
            context.Message.RegisteredAt);

        await publishEndpoint.Publish(
            new SendEmailRequestedEvent(
                context.Message.UserId,
                context.Message.Email,
                EmailType.ConfirmAccountEmail),
            context.CancellationToken);
    }
}
