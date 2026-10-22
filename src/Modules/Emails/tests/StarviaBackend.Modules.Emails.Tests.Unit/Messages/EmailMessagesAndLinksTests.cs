using FluentAssertions;
using StarviaBackend.Modules.Emails.Application.Emails.Links;
using StarviaBackend.Modules.Emails.Application.Emails.Messages;
using StarviaBackend.Modules.Emails.Application.Emails.Queries.BrowseEmails;
using StarviaBackend.Modules.Emails.Core.Emails.Exceptions;
using StarviaBackend.Modules.Emails.Tests.Unit.Fakes;
using StarviaBackend.Shared.Infrastructure.Auth;

namespace StarviaBackend.Modules.Emails.Tests.Unit.Messages;

public sealed class AccountEmailLinkTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-2222-3333-4444-555555555555");

    [Fact]
    public void ConfirmAccount_points_at_the_frontend_confirmation_page()
    {
        var link = AccountEmailLink.ConfirmAccount("https://app.example.com", UserId, "token");

        link.Should().StartWith($"https://app.example.com/email-confirmed?userId={UserId}&code=");
    }

    [Fact]
    public void ResetPassword_points_at_the_frontend_reset_page()
    {
        var link = AccountEmailLink.ResetPassword("https://app.example.com", UserId, "token");

        link.Should().StartWith($"https://app.example.com/reset-password?userId={UserId}&code=");
    }

    [Fact]
    public void Login_points_at_the_frontend_login_page()
    {
        AccountEmailLink.Login("https://app.example.com").Should().Be("https://app.example.com/login");
    }

    [Theory]
    [InlineData("https://app.example.com/")]
    [InlineData("https://app.example.com///")]
    public void Trailing_slashes_on_the_base_url_do_not_produce_double_slashes(string baseUrl)
    {
        AccountEmailLink.Login(baseUrl).Should().Be("https://app.example.com/login");
        AccountEmailLink.ConfirmAccount(baseUrl, UserId, "t").Should().StartWith("https://app.example.com/email-confirmed?");
        AccountEmailLink.ResetPassword(baseUrl, UserId, "t").Should().StartWith("https://app.example.com/reset-password?");
    }

    [Theory]
    [InlineData("plain")]
    [InlineData("CfDJ8+abc/def==")]
    [InlineData("a token with spaces & symbols?=#")]
    public void The_code_is_url_safe_and_round_trips_to_the_original_token(string token)
    {
        var link = AccountEmailLink.ConfirmAccount("https://app.example.com", UserId, token);

        var code = link[(link.IndexOf("code=", StringComparison.Ordinal) + "code=".Length)..];
        code.Should().NotContainAny("+", "/", "=", " ", "&", "?", "#");
        IdentityTokenEncoder.Decode(code).Should().Be(token);
    }
}

public sealed class AccountEmailMessagesTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task ConfirmAccountEmail_builds_the_link_from_the_users_confirmation_token()
    {
        var tokens = new FakeAccountTokenProvider { ConfirmationToken = "confirm-me" };
        var message = new ConfirmAccountEmail(tokens, new FakeAppUrls("https://app.example.com/"));

        var model = await message.CreateAsync(UserId, "user@example.com", CancellationToken.None);

        model.UserName.Should().Be("user@example.com");
        model.Link.Should().Be(AccountEmailLink.ConfirmAccount("https://app.example.com", UserId, "confirm-me"));
        tokens.ConfirmationRequests.Should().Equal(UserId);
        tokens.ResetRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetPasswordEmail_builds_the_link_from_the_users_reset_token()
    {
        var tokens = new FakeAccountTokenProvider { PasswordResetToken = "reset-me" };
        var message = new ResetPasswordEmail(tokens, new FakeAppUrls("https://app.example.com"));

        var model = await message.CreateAsync(UserId, "user@example.com", CancellationToken.None);

        model.UserName.Should().Be("user@example.com");
        model.Link.Should().Be(AccountEmailLink.ResetPassword("https://app.example.com", UserId, "reset-me"));
        tokens.ResetRequests.Should().Equal(UserId);
        tokens.ConfirmationRequests.Should().BeEmpty();
    }

    [Fact]
    public void WelcomingEmail_links_to_the_login_page_and_needs_no_token()
    {
        var message = new WelcomingEmail(new FakeAppUrls("https://app.example.com/"));

        var model = message.Create("user@example.com");

        model.UserName.Should().Be("user@example.com");
        model.Link.Should().Be("https://app.example.com/login");
    }

    [Fact]
    public async Task A_rejected_token_request_is_not_swallowed()
    {
        var tokens = new FakeAccountTokenProvider
        {
            Failure = new AccountTokenRejectedException(UserId, "user_not_found", "gone"),
        };

        var confirm = () => new ConfirmAccountEmail(tokens, new FakeAppUrls()).CreateAsync(UserId, "u@example.com", CancellationToken.None);
        var reset = () => new ResetPasswordEmail(tokens, new FakeAppUrls()).CreateAsync(UserId, "u@example.com", CancellationToken.None);

        await confirm.Should().ThrowAsync<AccountTokenRejectedException>();
        await reset.Should().ThrowAsync<AccountTokenRejectedException>();
    }

    [Fact]
    public void Every_email_has_a_non_empty_subject()
    {
        ConfirmAccountEmail.Subject.Should().NotBeNullOrWhiteSpace();
        ResetPasswordEmail.Subject.Should().NotBeNullOrWhiteSpace();
        WelcomingEmail.Subject.Should().NotBeNullOrWhiteSpace();
    }
}

public sealed class BrowseEmailsQueryTests
{
    [Fact]
    public void Defaults_to_the_first_page_of_twenty()
    {
        var query = new BrowseEmailsQuery();

        query.Page.Should().Be(1);
        query.PageSize.Should().Be(20);
    }

    [Theory]
    [InlineData(3, 50, 3, 50)]
    [InlineData(1, 1, 1, 1)]
    [InlineData(1, 100, 1, 100)]
    public void Valid_paging_is_kept(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        var query = new BrowseEmailsQuery(page, pageSize);

        query.Page.Should().Be(expectedPage);
        query.PageSize.Should().Be(expectedPageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void A_page_below_one_falls_back_to_the_first_page(int page)
    {
        new BrowseEmailsQuery(page, 10).Page.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(10_000)]
    public void A_page_size_outside_one_to_hundred_falls_back_to_twenty(int pageSize)
    {
        new BrowseEmailsQuery(1, pageSize).PageSize.Should().Be(20);
    }
}
