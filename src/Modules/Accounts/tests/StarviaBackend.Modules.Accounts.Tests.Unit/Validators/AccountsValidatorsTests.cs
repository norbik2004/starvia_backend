using FluentValidation.TestHelper;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ChangeUserName;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ConfirmEmail;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.GenerateEmailConfirmationToken;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.GeneratePasswordResetToken;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.RegisterUser;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.RequestPasswordReset;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ResetPassword;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.SignIn;

namespace StarviaBackend.Modules.Accounts.Tests.Unit.Validators;

public sealed class RegisterUserValidatorTests
{
    private readonly RegisterUserValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(new RegisterUserCommand("user@example.com", "Passw0rd!"))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    public void Invalid_email_fails(string email)
    {
        _validator.TestValidate(new RegisterUserCommand(email, "Passw0rd!"))
            .ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void Empty_or_short_password_fails(string password)
    {
        _validator.TestValidate(new RegisterUserCommand("user@example.com", password))
            .ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_of_exactly_six_characters_passes()
    {
        _validator.TestValidate(new RegisterUserCommand("user@example.com", "123456"))
            .ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}

public sealed class SignInValidatorTests
{
    private readonly SignInValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(new SignInCommand("user@example.com", "anything"))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("nope")]
    public void Invalid_email_fails(string email)
    {
        _validator.TestValidate(new SignInCommand(email, "anything"))
            .ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Empty_password_fails()
    {
        _validator.TestValidate(new SignInCommand("user@example.com", ""))
            .ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Short_password_is_not_rejected_at_sign_in()
    {
        // Length rules belong to registration/reset; sign-in must just compare against what exists.
        _validator.TestValidate(new SignInCommand("user@example.com", "a"))
            .ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}

public sealed class ChangeUserNameValidatorTests
{
    private readonly ChangeUserNameValidator _validator = new();

    [Theory]
    [InlineData("abc")]
    [InlineData("Normal")]
    [InlineData("abcdefghijklmnopqrstuvwxy")]
    public void Letters_only_names_of_three_to_twenty_five_characters_pass(string userName)
    {
        _validator.TestValidate(new ChangeUserNameCommand(userName, Guid.NewGuid()))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("abcdefghijklmnopqrstuvwxyz")]
    public void Names_outside_three_to_twenty_five_characters_fail(string userName)
    {
        _validator.TestValidate(new ChangeUserNameCommand(userName, Guid.NewGuid()))
            .ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Theory]
    [InlineData("abc1")]
    [InlineData("ab cd")]
    [InlineData("under_score")]
    [InlineData("zażółć")]
    [InlineData("user@example.com")]
    public void Names_with_anything_but_ascii_letters_fail(string userName)
    {
        _validator.TestValidate(new ChangeUserNameCommand(userName, Guid.NewGuid()))
            .ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Empty_user_id_fails()
    {
        _validator.TestValidate(new ChangeUserNameCommand("Valid", Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.UserId);
    }
}

public sealed class ConfirmEmailValidatorTests
{
    private readonly ConfirmEmailValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(new ConfirmEmailCommand(Guid.NewGuid(), "code"))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_user_id_fails()
    {
        _validator.TestValidate(new ConfirmEmailCommand(Guid.Empty, "code"))
            .ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Blank_code_fails(string code)
    {
        _validator.TestValidate(new ConfirmEmailCommand(Guid.NewGuid(), code))
            .ShouldHaveValidationErrorFor(x => x.Code);
    }
}

public sealed class ResetPasswordValidatorTests
{
    private readonly ResetPasswordValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(new ResetPasswordCommand(Guid.NewGuid(), "code", "N3wPassw0rd!"))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_user_id_fails()
    {
        _validator.TestValidate(new ResetPasswordCommand(Guid.Empty, "code", "N3wPassw0rd!"))
            .ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Empty_code_fails()
    {
        _validator.TestValidate(new ResetPasswordCommand(Guid.NewGuid(), "", "N3wPassw0rd!"))
            .ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void Empty_or_short_password_fails(string password)
    {
        _validator.TestValidate(new ResetPasswordCommand(Guid.NewGuid(), "code", password))
            .ShouldHaveValidationErrorFor(x => x.Password);
    }
}

public sealed class RequestPasswordResetValidatorTests
{
    private readonly RequestPasswordResetValidator _validator = new();

    [Fact]
    public void Valid_email_passes()
    {
        _validator.TestValidate(new RequestPasswordResetCommand("user@example.com"))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Invalid_email_fails(string email)
    {
        _validator.TestValidate(new RequestPasswordResetCommand(email))
            .ShouldHaveValidationErrorFor(x => x.Email);
    }
}

public sealed class TokenGenerationValidatorsTests
{
    [Fact]
    public void Email_confirmation_token_requires_a_user_id()
    {
        var validator = new GenerateEmailConfirmationTokenValidator();

        validator.TestValidate(new GenerateEmailConfirmationTokenCommand(Guid.NewGuid()))
            .ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new GenerateEmailConfirmationTokenCommand(Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Password_reset_token_requires_a_user_id()
    {
        var validator = new GeneratePasswordResetTokenValidator();

        validator.TestValidate(new GeneratePasswordResetTokenCommand(Guid.NewGuid()))
            .ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new GeneratePasswordResetTokenCommand(Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.UserId);
    }
}
