using StarviaBackend.Modules.Emails.Application.Emails.Templates;
using StarviaBackend.Modules.Emails.Application.Emails.Tokens;
using StarviaBackend.Modules.Emails.Core.Emails.Entities;
using StarviaBackend.Modules.Emails.Core.Emails.Repositories;
using StarviaBackend.Shared.Abstractions.App;
using StarviaBackend.Shared.Abstractions.Email;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Emails.Tests.Unit.Fakes;

internal sealed class FakeClock(DateTime utcNow) : IClock
{
    public DateTime UtcNow { get; set; } = utcNow;
}

internal sealed class FakeAppUrls(string frontendBaseUrl = "https://app.example.com", string apiBaseUrl = "https://api.example.com")
    : IAppUrls
{
    public string ApiBaseUrl { get; } = apiBaseUrl;

    public string FrontendBaseUrl { get; } = frontendBaseUrl;
}

internal sealed class InMemoryEmailRepository : IEmailRepository
{
    public List<Email> Emails { get; } = [];

    public Task AddAsync(Email email, CancellationToken cancellationToken = default)
    {
        Emails.Add(email);
        return Task.CompletedTask;
    }
}

internal sealed class FakeEmailSender : IEmailSender
{
    public List<EmailMessage> Sent { get; } = [];

    public List<CancellationToken> Tokens { get; } = [];

    /// <summary>When set, <see cref="SendAsync"/> throws it (simulates an SMTP failure).</summary>
    public Exception? Failure { get; set; }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (Failure is not null)
        {
            throw Failure;
        }

        Sent.Add(message);
        Tokens.Add(cancellationToken);
        return Task.CompletedTask;
    }
}

internal sealed class FakeEmailTemplateRenderer : IEmailTemplateRenderer
{
    public List<(EmailType Type, Guid UserId, string Email, CancellationToken Token)> Calls { get; } = [];

    public EmailTemplate Template { get; set; } = new("Subject", "<p>Body</p>");

    /// <summary>When set, <see cref="RenderAsync"/> throws it (e.g. the account token was rejected).</summary>
    public Exception? Failure { get; set; }

    public Task<EmailTemplate> RenderAsync(
        EmailType emailType,
        Guid userId,
        string email,
        CancellationToken cancellationToken = default)
    {
        if (Failure is not null)
        {
            throw Failure;
        }

        Calls.Add((emailType, userId, email, cancellationToken));
        return Task.FromResult(Template);
    }
}

internal sealed class FakeAccountTokenProvider : IAccountTokenProvider
{
    public string ConfirmationToken { get; set; } = "confirmation+token/with=chars";

    public string PasswordResetToken { get; set; } = "reset+token/with=chars";

    public List<Guid> ConfirmationRequests { get; } = [];

    public List<Guid> ResetRequests { get; } = [];

    /// <summary>When set, both methods throw it (simulates Accounts rejecting the request).</summary>
    public Exception? Failure { get; set; }

    public Task<string> GetEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (Failure is not null)
        {
            throw Failure;
        }

        ConfirmationRequests.Add(userId);
        return Task.FromResult(ConfirmationToken);
    }

    public Task<string> GetPasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (Failure is not null)
        {
            throw Failure;
        }

        ResetRequests.Add(userId);
        return Task.FromResult(PasswordResetToken);
    }
}
