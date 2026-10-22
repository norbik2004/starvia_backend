using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ChangeUserName;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ConfirmEmail;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.GenerateEmailConfirmationToken;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.GeneratePasswordResetToken;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.RegisterUser;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.RequestPasswordReset;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ResetPassword;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.SignIn;
using StarviaBackend.Modules.Accounts.Application.Users.Events.UserRegistered;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Modules.Accounts.Tests.Unit.Fakes;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Email;
using StarviaBackend.Shared.Infrastructure.Auth;

namespace StarviaBackend.Modules.Accounts.Tests.Unit.Handlers;

public abstract class HandlerTestBase
{
    protected static readonly DateTime Now = new(2026, 10, 20, 9, 0, 0, DateTimeKind.Utc);

    private protected FakeClock Clock { get; } = new(Now);

    private protected FakeUserManager UserManager { get; } = new();

    private protected InMemoryUsersRepository Repository { get; } = new();

    private protected RecordingPublishEndpoint Bus { get; } = new();

    /// <summary>Adds a user to both the repository and the manager's lookup, like a persisted account.</summary>
    private protected User AddUser(string email = "user@example.com", bool confirmed = false)
    {
        var user = User.Create(email, Now.AddDays(-1));

        if (confirmed)
        {
            user.ConfirmEmail();
        }

        Repository.Users.Add(user);
        UserManager.KnownUsers.Add(user);
        return user;
    }

    protected static string Encode(string token) => IdentityTokenEncoder.Encode(token);
}

public sealed class RegisterUserHandlerTests : HandlerTestBase
{
    private RegisterUserHandler Handler => new(UserManager, Repository, Bus, Clock);

    [Fact]
    public async Task Creates_an_unconfirmed_account_with_the_given_email_and_password()
    {
        await Handler.HandleAsync(new RegisterUserCommand("new@example.com", "Passw0rd!"));

        var user = UserManager.Created.Should().ContainSingle().Subject;
        user.Email.Should().Be("new@example.com");
        user.UserName.Should().Be("new@example.com");
        user.CreatedAt.Should().Be(Now);
        user.EmailConfirmed.Should().BeFalse();
        UserManager.CreatedWithPassword.Should().Equal("Passw0rd!");
    }

    [Fact]
    public async Task Gives_the_new_account_the_user_role()
    {
        await Handler.HandleAsync(new RegisterUserCommand("new@example.com", "Passw0rd!"));

        UserManager.RoleAssignments.Should().ContainSingle()
            .Which.Should().Be((UserManager.Created.Single(), UserRoles.User));
    }

    [Fact]
    public async Task Publishes_UserRegistered_and_returns_the_new_user_id()
    {
        var result = await Handler.HandleAsync(new RegisterUserCommand("new@example.com", "Passw0rd!"));

        var user = UserManager.Created.Single();
        result.UserId.Should().Be(user.Id);
        Bus.PublishedOf<UserRegisteredEvent>().Should().ContainSingle()
            .Which.Should().Be(new UserRegisteredEvent(user.Id, "new@example.com", Now));
    }

    [Theory]
    [InlineData("taken@example.com")]
    [InlineData("TAKEN@EXAMPLE.COM")]
    public async Task An_email_already_in_use_is_rejected_regardless_of_case(string email)
    {
        AddUser("taken@example.com");

        var act = () => Handler.HandleAsync(new RegisterUserCommand(email, "Passw0rd!"));

        await act.Should().ThrowAsync<EmailAlreadyInUseException>();
        UserManager.Created.Should().BeEmpty();
        Bus.Published.Should().BeEmpty();
    }

    [Fact]
    public async Task Identity_failure_is_reported_with_all_error_descriptions_and_nothing_is_published()
    {
        UserManager.CreateResult = IdentityResult.Failed(
            new IdentityError { Code = "A", Description = "first problem" },
            new IdentityError { Code = "B", Description = "second problem" });

        var act = () => Handler.HandleAsync(new RegisterUserCommand("new@example.com", "Passw0rd!"));

        var exception = (await act.Should().ThrowAsync<UserCreationFailedException>()).Which;
        exception.Message.Should().Contain("first problem; second problem");
        UserManager.RoleAssignments.Should().BeEmpty();
        Bus.Published.Should().BeEmpty();
    }
}

public sealed class ChangeUserNameHandlerTests : HandlerTestBase
{
    private ChangeUserNameHandler Handler => new(Repository);

    [Fact]
    public async Task Renames_the_user_and_saves_it()
    {
        var user = AddUser();

        var result = await Handler.HandleAsync(new ChangeUserNameCommand("Newname", user.Id));

        user.UserName.Should().Be("Newname");
        Repository.Updated.Should().ContainSingle().Which.Should().BeSameAs(user);
        result.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Unknown_user_is_reported_and_nothing_is_saved()
    {
        var act = () => Handler.HandleAsync(new ChangeUserNameCommand("Newname", Guid.NewGuid()));

        await act.Should().ThrowAsync<UserNotFoundException>();
        Repository.Updated.Should().BeEmpty();
    }
}

public sealed class ConfirmEmailHandlerTests : HandlerTestBase
{
    private ConfirmEmailHandler Handler => new(UserManager, Repository, Bus);

    [Fact]
    public async Task A_valid_code_confirms_the_email_using_the_decoded_token()
    {
        var user = AddUser();

        await Handler.HandleAsync(new ConfirmEmailCommand(user.Id, Encode("raw+token/value")));

        UserManager.ConfirmAttempts.Should().ContainSingle()
            .Which.Should().Be((user, "raw+token/value"));
        user.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task Confirming_queues_the_welcoming_email()
    {
        var user = AddUser("welcome@example.com");

        await Handler.HandleAsync(new ConfirmEmailCommand(user.Id, Encode("token")));

        Bus.PublishedOf<SendEmailRequestedEvent>().Should().ContainSingle()
            .Which.Should().Be(new SendEmailRequestedEvent(user.Id, "welcome@example.com", EmailType.WelcomingEmail));
    }

    [Fact]
    public async Task An_already_confirmed_email_is_a_silent_no_op()
    {
        var user = AddUser(confirmed: true);

        await Handler.HandleAsync(new ConfirmEmailCommand(user.Id, Encode("token")));

        UserManager.ConfirmAttempts.Should().BeEmpty();
        Bus.Published.Should().BeEmpty();
    }

    [Fact]
    public async Task Unknown_user_is_reported()
    {
        var act = () => Handler.HandleAsync(new ConfirmEmailCommand(Guid.NewGuid(), Encode("token")));

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task A_code_that_is_not_valid_base64_is_rejected_before_asking_identity()
    {
        var user = AddUser();

        var act = () => Handler.HandleAsync(new ConfirmEmailCommand(user.Id, "!!!"));

        await act.Should().ThrowAsync<InvalidEmailConfirmationCodeException>();
        UserManager.ConfirmAttempts.Should().BeEmpty();
        Bus.Published.Should().BeEmpty();
    }

    [Fact]
    public async Task A_token_identity_refuses_is_reported_and_leaves_the_email_unconfirmed()
    {
        var user = AddUser();
        UserManager.ConfirmResult = IdentityResult.Failed(new IdentityError { Code = "InvalidToken" });

        var act = () => Handler.HandleAsync(new ConfirmEmailCommand(user.Id, Encode("stale")));

        await act.Should().ThrowAsync<InvalidEmailConfirmationCodeException>();
        user.EmailConfirmed.Should().BeFalse();
        Bus.Published.Should().BeEmpty();
    }
}

public sealed class GenerateTokenHandlersTests : HandlerTestBase
{
    [Fact]
    public async Task Email_confirmation_token_comes_from_identity()
    {
        var user = AddUser();
        UserManager.EmailConfirmationToken = "the-confirmation-token";

        var result = await new GenerateEmailConfirmationTokenHandler(UserManager, Repository)
            .HandleAsync(new GenerateEmailConfirmationTokenCommand(user.Id));

        result.Token.Should().Be("the-confirmation-token");
    }

    [Fact]
    public async Task Email_confirmation_token_for_a_confirmed_user_is_refused()
    {
        var user = AddUser(confirmed: true);

        var act = () => new GenerateEmailConfirmationTokenHandler(UserManager, Repository)
            .HandleAsync(new GenerateEmailConfirmationTokenCommand(user.Id));

        await act.Should().ThrowAsync<EmailArleadyConfirmedException>();
    }

    [Fact]
    public async Task Email_confirmation_token_for_an_unknown_user_is_refused()
    {
        var act = () => new GenerateEmailConfirmationTokenHandler(UserManager, Repository)
            .HandleAsync(new GenerateEmailConfirmationTokenCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task Password_reset_token_comes_from_identity_even_for_unconfirmed_users()
    {
        var user = AddUser(confirmed: false);
        UserManager.PasswordResetToken = "the-reset-token";

        var result = await new GeneratePasswordResetTokenHandler(UserManager, Repository)
            .HandleAsync(new GeneratePasswordResetTokenCommand(user.Id));

        result.Token.Should().Be("the-reset-token");
    }

    [Fact]
    public async Task Password_reset_token_for_an_unknown_user_is_refused()
    {
        var act = () => new GeneratePasswordResetTokenHandler(UserManager, Repository)
            .HandleAsync(new GeneratePasswordResetTokenCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<UserNotFoundException>();
    }
}

public sealed class RequestPasswordResetHandlerTests : HandlerTestBase
{
    private RequestPasswordResetHandler Handler => new(Repository, Bus);

    [Fact]
    public async Task A_known_email_queues_the_reset_password_email()
    {
        var user = AddUser("known@example.com");

        await Handler.HandleAsync(new RequestPasswordResetCommand("known@example.com"));

        Bus.PublishedOf<SendEmailRequestedEvent>().Should().ContainSingle()
            .Which.Should().Be(new SendEmailRequestedEvent(user.Id, "known@example.com", EmailType.ResetPasswordEmail));
    }

    [Fact]
    public async Task The_email_is_matched_case_insensitively_but_sent_as_typed()
    {
        var user = AddUser("known@example.com");

        await Handler.HandleAsync(new RequestPasswordResetCommand("KNOWN@example.com"));

        Bus.PublishedOf<SendEmailRequestedEvent>().Should().ContainSingle()
            .Which.Should().Be(new SendEmailRequestedEvent(user.Id, "KNOWN@example.com", EmailType.ResetPasswordEmail));
    }

    [Fact]
    public async Task An_unknown_email_completes_silently_without_revealing_anything()
    {
        await Handler.HandleAsync(new RequestPasswordResetCommand("nobody@example.com"));

        Bus.Published.Should().BeEmpty();
    }
}

public sealed class ResetPasswordHandlerTests : HandlerTestBase
{
    private ResetPasswordHandler Handler => new(UserManager, Repository);

    [Fact]
    public async Task A_valid_code_resets_the_password_using_the_decoded_token()
    {
        var user = AddUser();

        await Handler.HandleAsync(new ResetPasswordCommand(user.Id, Encode("raw+token/value"), "N3wPassw0rd!"));

        UserManager.ResetAttempts.Should().ContainSingle()
            .Which.Should().Be((user, "raw+token/value", "N3wPassw0rd!"));
    }

    [Fact]
    public async Task Unknown_user_is_reported()
    {
        var act = () => Handler.HandleAsync(new ResetPasswordCommand(Guid.NewGuid(), Encode("t"), "N3wPassw0rd!"));

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task A_code_that_is_not_valid_base64_is_rejected_before_asking_identity()
    {
        var user = AddUser();

        var act = () => Handler.HandleAsync(new ResetPasswordCommand(user.Id, "!!!", "N3wPassw0rd!"));

        await act.Should().ThrowAsync<InvalidPasswordResetCodeException>();
        UserManager.ResetAttempts.Should().BeEmpty();
    }

    [Fact]
    public async Task An_invalid_token_error_becomes_an_invalid_code_failure()
    {
        var user = AddUser();
        UserManager.ResetResult = IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "bad token" });

        var act = () => Handler.HandleAsync(new ResetPasswordCommand(user.Id, Encode("stale"), "N3wPassw0rd!"));

        await act.Should().ThrowAsync<InvalidPasswordResetCodeException>();
    }

    [Fact]
    public async Task Any_other_identity_error_is_reported_with_its_descriptions()
    {
        var user = AddUser();
        UserManager.ResetResult = IdentityResult.Failed(
            new IdentityError { Code = "PasswordTooShort", Description = "too short" },
            new IdentityError { Code = "PasswordRequiresDigit", Description = "needs a digit" });

        var act = () => Handler.HandleAsync(new ResetPasswordCommand(user.Id, Encode("token"), "weak"));

        var exception = (await act.Should().ThrowAsync<PasswordResetFailedException>()).Which;
        exception.Message.Should().Contain("too short; needs a digit");
    }
}

public sealed class SignInHandlerTests : HandlerTestBase
{
    private readonly FakeAuthManager _auth = new();

    private SignInHandler Handler => new(UserManager, _auth, Clock);

    private User AddUserWithPassword(string email = "user@example.com", string password = "Passw0rd!", bool confirmed = true)
    {
        var user = AddUser(email, confirmed);
        UserManager.Passwords[user.Id] = password;
        return user;
    }

    [Fact]
    public async Task Valid_credentials_return_a_token_carrying_the_users_identity_and_roles()
    {
        var user = AddUserWithPassword();
        UserManager.Roles[user.Id] = [UserRoles.User, UserRoles.Admin];

        var token = await Handler.HandleAsync(new SignInCommand("user@example.com", "Passw0rd!"));

        token.UserId.Should().Be(user.Id.ToString());
        token.Email.Should().Be("user@example.com");
        token.Roles.Should().BeEquivalentTo([UserRoles.User, UserRoles.Admin]);
        _auth.Issued.Should().ContainSingle();
    }

    [Fact]
    public async Task Signing_in_records_the_login_time()
    {
        var user = AddUserWithPassword();

        await Handler.HandleAsync(new SignInCommand("user@example.com", "Passw0rd!"));

        user.LastLoginAt.Should().Be(Now);
    }

    [Fact]
    public async Task Unknown_email_is_reported_as_invalid_credentials()
    {
        var act = () => Handler.HandleAsync(new SignInCommand("nobody@example.com", "Passw0rd!"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
        _auth.Issued.Should().BeEmpty();
    }

    [Fact]
    public async Task Wrong_password_is_reported_as_invalid_credentials_and_leaves_no_trace()
    {
        var user = AddUserWithPassword();

        var act = () => Handler.HandleAsync(new SignInCommand("user@example.com", "wrong"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
        user.LastLoginAt.Should().BeNull();
        _auth.Issued.Should().BeEmpty();
    }

    [Fact]
    public async Task An_unconfirmed_email_blocks_sign_in()
    {
        var user = AddUserWithPassword(confirmed: false);

        var act = () => Handler.HandleAsync(new SignInCommand("user@example.com", "Passw0rd!"));

        await act.Should().ThrowAsync<EmailNotConfirmedException>();
        user.LastLoginAt.Should().BeNull();
        _auth.Issued.Should().BeEmpty();
    }

    [Fact]
    public async Task The_confirmation_state_is_only_revealed_after_the_password_checks_out()
    {
        AddUserWithPassword(confirmed: false);

        var act = () => Handler.HandleAsync(new SignInCommand("user@example.com", "wrong"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }
}
