
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Emails.Application.Emails.Commands.SendEmail;

public sealed record SendEmailCommand(SendEmailRequest request, Guid UserId)
    : ICommand<SendEmailResult>;

public sealed record SendEmailRequest(
    string Email,
    EmailType EmailType);

public sealed record SendEmailResult(Guid EmailId);
