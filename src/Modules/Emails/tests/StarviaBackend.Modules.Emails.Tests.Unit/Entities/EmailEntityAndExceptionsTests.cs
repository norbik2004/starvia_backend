using FluentAssertions;
using StarviaBackend.Modules.Emails.Core.Emails.Entities;
using StarviaBackend.Modules.Emails.Core.Emails.Exceptions;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Emails.Tests.Unit.Entities;

public sealed class EmailEntityTests
{
    [Fact]
    public void Create_stores_every_value_it_is_given()
    {
        var sentAt = new DateTime(2026, 10, 20, 12, 30, 0, DateTimeKind.Utc);

        var email = Email.Create("Hello", "<p>Hi</p>", sentAt, "user@example.com", EmailType.ConfirmAccountEmail);

        email.Title.Should().Be("Hello");
        email.Body.Should().Be("<p>Hi</p>");
        email.SentAt.Should().Be(sentAt);
        email.SentTo.Should().Be("user@example.com");
        email.Type.Should().Be(EmailType.ConfirmAccountEmail);
    }

    [Fact]
    public void Create_generates_a_new_non_empty_id_each_time()
    {
        var first = Email.Create("t", "b", DateTime.UtcNow, "a@example.com", EmailType.WelcomingEmail);
        var second = Email.Create("t", "b", DateTime.UtcNow, "a@example.com", EmailType.WelcomingEmail);

        first.Id.Should().NotBeEmpty();
        second.Id.Should().NotBeEmpty();
        first.Id.Should().NotBe(second.Id);
    }

    [Theory]
    [InlineData(EmailType.WelcomingEmail)]
    [InlineData(EmailType.ConfirmAccountEmail)]
    [InlineData(EmailType.ResetPasswordEmail)]
    public void Create_keeps_the_email_type(EmailType type)
    {
        var email = Email.Create("t", "b", DateTime.UtcNow, "a@example.com", type);

        email.Type.Should().Be(type);
    }
}

public sealed class EmailExceptionsTests
{
    [Fact]
    public void EmailNotSent_has_a_stable_code_and_names_the_recipient()
    {
        var exception = new EmailNotSentException("user@example.com");

        exception.Code.Should().Be("email_not_sent");
        exception.Message.Should().Contain("user@example.com");
    }

    [Fact]
    public void AccountTokenRejected_exposes_the_rejection_code_and_names_the_user()
    {
        var userId = Guid.NewGuid();

        var exception = new AccountTokenRejectedException(userId, "email_already_confirmed", "nothing to do");

        exception.Code.Should().Be("account_token_rejected");
        exception.RejectionCode.Should().Be("email_already_confirmed");
        exception.Message.Should().Contain(userId.ToString())
            .And.Contain("email_already_confirmed")
            .And.Contain("nothing to do");
    }
}
