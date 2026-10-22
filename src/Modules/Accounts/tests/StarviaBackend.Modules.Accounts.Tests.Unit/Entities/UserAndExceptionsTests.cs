using FluentAssertions;
using StarviaBackend.Modules.Accounts.Application.Users.Queries.BrowseUsers;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Accounts.Tests.Unit.Entities;

public sealed class UserTests
{
    private static readonly DateTime CreatedAt = new(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc);

    [Fact]
    public void Create_uses_the_email_as_both_email_and_user_name()
    {
        var user = User.Create("user@example.com", CreatedAt);

        user.Email.Should().Be("user@example.com");
        user.UserName.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_starts_unconfirmed_and_never_logged_in()
    {
        var user = User.Create("user@example.com", CreatedAt);

        user.CreatedAt.Should().Be(CreatedAt);
        user.EmailConfirmed.Should().BeFalse();
        user.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void Create_starts_without_roles_claims_or_logins()
    {
        var user = User.Create("user@example.com", CreatedAt);

        user.Roles.Should().BeEmpty();
        user.Claims.Should().BeEmpty();
        user.Logins.Should().BeEmpty();
    }

    [Fact]
    public void Create_generates_a_distinct_non_empty_id_per_user()
    {
        var first = User.Create("a@example.com", CreatedAt);
        var second = User.Create("a@example.com", CreatedAt);

        first.Id.Should().NotBeEmpty();
        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public void ChangeUserName_replaces_the_user_name_but_not_the_email()
    {
        var user = User.Create("user@example.com", CreatedAt);

        user.ChangeUserName("Newname");

        user.UserName.Should().Be("Newname");
        user.Email.Should().Be("user@example.com");
    }

    [Fact]
    public void RecordLogin_stores_the_login_time()
    {
        var user = User.Create("user@example.com", CreatedAt);
        var loginAt = CreatedAt.AddDays(2);

        user.RecordLogin(loginAt);

        user.LastLoginAt.Should().Be(loginAt);
    }

    [Fact]
    public void RecordLogin_keeps_only_the_latest_login()
    {
        var user = User.Create("user@example.com", CreatedAt);

        user.RecordLogin(CreatedAt.AddDays(1));
        user.RecordLogin(CreatedAt.AddDays(5));

        user.LastLoginAt.Should().Be(CreatedAt.AddDays(5));
    }

    [Fact]
    public void ConfirmEmail_marks_the_email_confirmed()
    {
        var user = User.Create("user@example.com", CreatedAt);

        user.ConfirmEmail();

        user.EmailConfirmed.Should().BeTrue();
    }
}

public sealed class AccountsExceptionsTests
{
    public static TheoryData<BusinessException, string> Exceptions => new()
    {
        { new EmailAlreadyInUseException("a@b.com"), "email_already_in_use" },
        { new UserNotFoundException(Guid.NewGuid()), "user_not_found" },
        { new InvalidCredentialsException(), "invalid_credentials" },
        { new UserCreationFailedException("weak"), "user_creation_failed" },
        { new EmailNotConfirmedException(), "email_not_confirmed" },
        { new EmailArleadyConfirmedException("a@b.com"), "email_arleady_confirmed" },
        { new InvalidEmailConfirmationCodeException(), "invalid_email_confirmation_code" },
        { new InvalidPasswordResetCodeException(), "invalid_password_reset_code" },
        { new PasswordResetFailedException("weak"), "password_reset_failed" },
    };

    [Theory]
    [MemberData(nameof(Exceptions))]
    public void Each_exception_exposes_its_stable_error_code(BusinessException exception, string expectedCode)
    {
        exception.Code.Should().Be(expectedCode);
        exception.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Exceptions_that_describe_a_subject_name_it_in_the_message()
    {
        var userId = Guid.NewGuid();

        new EmailAlreadyInUseException("a@b.com").Message.Should().Contain("a@b.com");
        new UserNotFoundException(userId).Message.Should().Contain(userId.ToString());
        new EmailArleadyConfirmedException("a@b.com").Message.Should().Contain("a@b.com");
        new UserCreationFailedException("too weak").Message.Should().Contain("too weak");
        new PasswordResetFailedException("too weak").Message.Should().Contain("too weak");
    }
}

public sealed class BrowseUsersQueryTests
{
    [Fact]
    public void Defaults_to_the_first_page_of_twenty()
    {
        var query = new BrowseUsersQuery();

        query.Page.Should().Be(1);
        query.PageSize.Should().Be(20);
    }

    [Theory]
    [InlineData(3, 50)]
    [InlineData(1, 1)]
    [InlineData(1, 100)]
    public void Valid_paging_is_kept(int page, int pageSize)
    {
        var query = new BrowseUsersQuery(page, pageSize);

        query.Page.Should().Be(page);
        query.PageSize.Should().Be(pageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void A_page_below_one_falls_back_to_the_first_page(int page)
    {
        new BrowseUsersQuery(page, 10).Page.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void A_page_size_outside_one_to_hundred_falls_back_to_twenty(int pageSize)
    {
        new BrowseUsersQuery(1, pageSize).PageSize.Should().Be(20);
    }
}
