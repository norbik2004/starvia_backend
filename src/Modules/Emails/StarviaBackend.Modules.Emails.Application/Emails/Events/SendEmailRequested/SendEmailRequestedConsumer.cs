using MassTransit;
using Microsoft.Extensions.Logging;
using StarviaBackend.Modules.Emails.Application.Emails.Commands.SendEmail;
using StarviaBackend.Modules.Emails.Core.Emails.Enums;
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
        if (!Enum.TryParse<EmailType>(message.EmailType, ignoreCase: true, out var emailType))
        {
            logger.LogError(
                "Unsupported email type {EmailType} for user {UserId}",
                message.EmailType,
                message.UserId);
            return;
        }

        logger.LogInformation(
            "Processing queued {EmailType} email for {Email} (user {UserId})",
            emailType,
            message.Email,
            message.UserId);

        await dispatcher.SendAsync<SendEmailCommand, SendEmailResult>(
            new SendEmailCommand(
                new SendEmailRequest(message.Email, emailType, message.Code, message.UserName, message.Link),
                message.UserId),
            context.CancellationToken);
    }
}
