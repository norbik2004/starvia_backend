using FluentValidation.TestHelper;
using StarviaBackend.Modules.Emails.Application.Emails.Commands.SendEmail;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Emails.Tests.Unit.Validators;

public sealed class SendEmailValidatorTests
{
    private readonly SendEmailValidator _validator = new();

    private static SendEmailCommand Command(string email = "user@example.com", EmailType type = EmailType.WelcomingEmail) =>
        new(new SendEmailRequest(email, type), Guid.NewGuid());

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(Command()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(EmailType.WelcomingEmail)]
    [InlineData(EmailType.ConfirmAccountEmail)]
    [InlineData(EmailType.ResetPasswordEmail)]
    public void Every_known_email_type_is_accepted(EmailType type)
    {
        _validator.TestValidate(Command(type: type)).ShouldNotHaveValidationErrorFor(x => x.request.EmailType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_email_fails(string email)
    {
        _validator.TestValidate(Command(email: email)).ShouldHaveValidationErrorFor(x => x.request.Email);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("@missing-local.com")]
    public void Malformed_email_fails(string email)
    {
        _validator.TestValidate(Command(email: email)).ShouldHaveValidationErrorFor(x => x.request.Email);
    }

    [Fact]
    public void Unknown_email_type_fails()
    {
        _validator.TestValidate(Command(type: (EmailType)999)).ShouldHaveValidationErrorFor(x => x.request.EmailType);
    }
}
