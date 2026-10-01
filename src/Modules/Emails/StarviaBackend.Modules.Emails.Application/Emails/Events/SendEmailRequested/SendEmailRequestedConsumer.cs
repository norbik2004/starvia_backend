using MassTransit;
using Microsoft.Extensions.Logging;
using StarviaBackend.Modules.Emails.Application.Emails.Commands.SendEmail;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Emails.Application.Emails.Events.SendEmailRequested;

internal sealed class SendEmailRequestedConsumer(
    IDispatcher dispatcher,
    ILogger<SendEmailRequestedConsumer> logger)
    : IConsumer<SendEmailRequestedEvent>
{
    public async Task Consume(ConsumeContext<SendEmailRequestedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Processing queued {EmailType} email for {Email} (user {UserId})",
            message.EmailType,
            message.Email,
            message.UserId);

        await dispatcher.SendAsync<SendEmailCommand, SendEmailResult>(
            new SendEmailCommand(
                new SendEmailRequest(message.Email, message.EmailType),
                message.UserId),
            context.CancellationToken);
    }
}
