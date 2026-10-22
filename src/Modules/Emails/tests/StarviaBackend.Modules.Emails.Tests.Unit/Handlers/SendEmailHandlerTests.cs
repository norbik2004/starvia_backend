using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using StarviaBackend.Modules.Emails.Application.Emails.Commands.SendEmail;
using StarviaBackend.Modules.Emails.Application.Emails.Templates;
using StarviaBackend.Modules.Emails.Core.Emails.Exceptions;
using StarviaBackend.Modules.Emails.Tests.Unit.Fakes;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Emails.Tests.Unit.Handlers;

public sealed class SendEmailHandlerTests
{
    private static readonly DateTime Now = new(2026, 10, 20, 8, 0, 0, DateTimeKind.Utc);

    private readonly FakeEmailTemplateRenderer _renderer = new()
    {
        Template = new EmailTemplate("Welcome!", "<html>welcome</html>"),
    };

    private readonly FakeEmailSender _sender = new();
    private readonly InMemoryEmailRepository _repository = new();
    private readonly FakeClock _clock = new(Now);
    private readonly SendEmailHandler _handler;

    public SendEmailHandlerTests()
    {
        _handler = new SendEmailHandler(_renderer, _sender, _repository, _clock, NullLogger<SendEmailHandler>.Instance);
    }

    private static SendEmailCommand Command(
        Guid? userId = null,
        string email = "user@example.com",
        EmailType type = EmailType.WelcomingEmail) =>
        new(new SendEmailRequest(email, type), userId ?? Guid.NewGuid());

    [Fact]
    public async Task Renders_the_template_for_the_requested_type_user_and_address()
    {
        var userId = Guid.NewGuid();

        await _handler.HandleAsync(Command(userId, "target@example.com", EmailType.ResetPasswordEmail));

        var call = _renderer.Calls.Should().ContainSingle().Subject;
        call.Type.Should().Be(EmailType.ResetPasswordEmail);
        call.UserId.Should().Be(userId);
        call.Email.Should().Be("target@example.com");
    }

    [Fact]
    public async Task Sends_the_rendered_subject_and_body_to_the_recipient()
    {
        await _handler.HandleAsync(Command(email: "target@example.com"));

        _sender.Sent.Should().ContainSingle().Which.Should().Be(
            new EmailMessage("target@example.com", "Welcome!", "<html>welcome</html>"));
    }

    [Fact]
    public async Task Stores_the_sent_email_stamped_with_the_clock_time()
    {
        await _handler.HandleAsync(Command(email: "target@example.com", type: EmailType.ConfirmAccountEmail));

        var stored = _repository.Emails.Should().ContainSingle().Subject;
        stored.Title.Should().Be("Welcome!");
        stored.Body.Should().Be("<html>welcome</html>");
        stored.SentTo.Should().Be("target@example.com");
        stored.Type.Should().Be(EmailType.ConfirmAccountEmail);
        stored.SentAt.Should().Be(Now);
    }

    [Fact]
    public async Task Returns_the_id_of_the_stored_email()
    {
        var result = await _handler.HandleAsync(Command());

        result.EmailId.Should().Be(_repository.Emails.Single().Id);
    }

    [Fact]
    public async Task Forwards_the_cancellation_token_to_renderer_and_sender()
    {
        using var cts = new CancellationTokenSource();

        await _handler.HandleAsync(Command(), cts.Token);

        _renderer.Calls.Single().Token.Should().Be(cts.Token);
        _sender.Tokens.Single().Should().Be(cts.Token);
    }

    [Fact]
    public async Task Smtp_failure_is_reported_as_EmailNotSent_and_nothing_is_stored()
    {
        _sender.Failure = new InvalidOperationException("smtp down");

        var act = () => _handler.HandleAsync(Command(email: "target@example.com"));

        var exception = (await act.Should().ThrowAsync<EmailNotSentException>()).Which;
        exception.Message.Should().Contain("target@example.com");
        _repository.Emails.Should().BeEmpty();
    }

    [Fact]
    public async Task Rendering_failure_propagates_and_nothing_is_sent_or_stored()
    {
        var userId = Guid.NewGuid();
        _renderer.Failure = new AccountTokenRejectedException(userId, "user_not_found", "gone");

        var act = () => _handler.HandleAsync(Command(userId, type: EmailType.ConfirmAccountEmail));

        await act.Should().ThrowAsync<AccountTokenRejectedException>();
        _sender.Sent.Should().BeEmpty();
        _repository.Emails.Should().BeEmpty();
    }
}
