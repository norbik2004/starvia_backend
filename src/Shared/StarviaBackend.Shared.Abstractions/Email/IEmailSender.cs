namespace StarviaBackend.Shared.Abstractions.Email;

/// <summary>Sends outbound email via SMTP. Implemented in Shared.Infrastructure.</summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? TextBody = null);
