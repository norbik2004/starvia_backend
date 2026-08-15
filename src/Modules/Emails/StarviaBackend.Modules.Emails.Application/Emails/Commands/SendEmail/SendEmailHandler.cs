using Microsoft.Extensions.Logging;
using StarviaBackend.Modules.Emails.Application.Emails.Templates;
using StarviaBackend.Modules.Emails.Core.Emails.Entities;
using StarviaBackend.Modules.Emails.Core.Emails.Exceptions;
using StarviaBackend.Modules.Emails.Core.Emails.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Email;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Emails.Application.Emails.Commands.SendEmail;

internal sealed class SendEmailHandler(
    IEmailTemplateRenderer templateRenderer,
    IEmailSender emailSender,
    IEmailRepository emailRepository,
    IClock clock,
    ILogger<SendEmailHandler> logger)
    : ICommandHandler<SendEmailCommand, SendEmailResult>
{
    public async Task<SendEmailResult> HandleAsync(SendEmailCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.request;
        var template = templateRenderer.Render(
            request.EmailType,
            new EmailTemplateModel(request.Email, request.Code, request.UserName));

        try
        {
            await emailSender.SendAsync(
                new EmailMessage(request.Email, template.Subject, template.HtmlBody),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send {EmailType} email to {Email}", request.EmailType, request.Email);
            throw new EmailNotSentException(request.Email);
        }

        var email = Email.Create(template.Subject, template.HtmlBody, clock.UtcNow, request.Email);
        await emailRepository.AddAsync(email, cancellationToken);

        return new SendEmailResult(email.Id);
    }
}
