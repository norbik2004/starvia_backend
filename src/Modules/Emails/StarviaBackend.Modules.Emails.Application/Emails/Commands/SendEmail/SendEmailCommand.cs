using StarviaBackend.Modules.Emails.Core.Emails.Enums;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Emails.Application.Emails.Commands.SendEmail;

public sealed record SendEmailCommand(SendEmailRequest request, Guid UserId)
    : ICommand<SendEmailResult>;

public sealed record SendEmailRequest(
    string Email,
    EmailType EmailType,
    string? Code = null,
    string? UserName = null);

public sealed record SendEmailResult(Guid EmailId);
