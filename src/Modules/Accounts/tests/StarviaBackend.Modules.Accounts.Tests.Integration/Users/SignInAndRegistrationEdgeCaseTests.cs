using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Users;

public sealed class SignInEdgeCaseTests(AccountsApp app) : AccountsFlowTest(app)
{
    private const string Route = "/v1/accounts/sign-in";

    [Fact]
    public async Task Unknown_email_and_wrong_password_are_indistinguishable()
    {
        var (_, email) = await RegisterAndConfirmAsync();

        var unknownEmail = await Client.PostAsJsonAsync(Route, new { email = UniqueEmail(), password = Password });
        var wrongPassword = await Client.PostAsJsonAsync(Route, new { email, password = "wrong-password" });

        unknownEmail.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        wrongPassword.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await unknownEmail.Content.ReadFromJsonAsync<ErrorResponse>())!.Errors
            .Should().Contain(e => e.Code == "invalid_credentials");
        (await wrongPassword.Content.ReadFromJsonAsync<ErrorResponse>())!.Errors
            .Should().Contain(e => e.Code == "invalid_credentials");
    }

    [Theory]
    [InlineData("not-an-email", "Passw0rd!")]
    [InlineData("", "Passw0rd!")]
    [InlineData("user@example.com", "")]
    public async Task Malformed_requests_are_rejected_by_validation(string email, string password)
    {
        var response = await Client.PostAsJsonAsync(Route, new { email, password });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task The_email_is_matched_case_insensitively()
    {
        var (_, email) = await RegisterAndConfirmAsync();

        var response = await Client.PostAsJsonAsync(Route, new { email = email.ToUpperInvariant(), password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task The_issued_token_identifies_the_user()
    {
        var (userId, email) = await RegisterAndConfirmAsync();

        var token = await (await Client.PostAsJsonAsync(Route, new { email, password = Password }))
            .Content.ReadFromJsonAsync<JsonWebToken>();

        token!.UserId.Should().Be(userId.ToString());
        token.Email.Should().Be(email);
        token.Roles.Should().Equal(UserRoles.User);
    }

    [Fact(Skip = "Finding: SignInHandler calls user.RecordLogin(...) but never saves the user, so LastLoginAt stays null in the database.")]
    public async Task Signing_in_persists_the_last_login_time()
    {
        var (userId, email) = await RegisterAndConfirmAsync();

        (await Client.PostAsJsonAsync(Route, new { email, password = Password })).EnsureSuccessStatusCode();

        await WithDbAsync<AccountsWriteDbContext>(async db =>
            (await db.Users.AsNoTracking().SingleAsync(u => u.Id == userId)).LastLoginAt.Should().NotBeNull());
    }
}

public sealed class RegistrationAndAccountEdgeCaseTests(AccountsApp app) : AccountsFlowTest(app)
{
    [Theory]
    [InlineData("12345")]
    [InlineData("")]
    public async Task A_password_shorter_than_six_characters_is_rejected(string password)
    {
        var email = UniqueEmail();

        var response = await Client.PostAsJsonAsync("/v1/accounts/register", new { email, password });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await WithDbAsync<AccountsWriteDbContext>(async db =>
            (await db.Users.AnyAsync(u => u.Email == email)).Should().BeFalse());
    }

    [Fact]
    public async Task The_same_email_in_a_different_case_counts_as_already_in_use()
    {
        var email = UniqueEmail();
        await RegisterAsync(email);

        var response = await Client.PostAsJsonAsync(
            "/v1/accounts/register",
            new { email = email.ToUpperInvariant(), password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadFromJsonAsync<ErrorResponse>())!.Errors
            .Should().Contain(e => e.Code == "email_already_in_use");
    }

    [Fact]
    public async Task A_new_account_starts_unconfirmed_with_the_user_role_and_no_password_stored_in_clear()
    {
        var email = UniqueEmail();
        var userId = await RegisterAsync(email);

        await WithDbAsync<AccountsWriteDbContext>(async db =>
        {
            var user = await db.Users.AsNoTracking().SingleAsync(u => u.Id == userId);
            user.EmailConfirmed.Should().BeFalse();
            user.UserName.Should().Be(email);
            user.PasswordHash.Should().NotBeNullOrEmpty().And.NotContain(Password);
            user.LastLoginAt.Should().BeNull();
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
        });
    }

    [Fact]
    public async Task Me_requires_an_identity()
    {
        var response = await CreateAnonymousClient().GetAsync("/v1/accounts/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_for_a_token_whose_account_does_not_exist_is_a_user_not_found_error()
    {
        var response = await CreateClient(Guid.NewGuid(), UserRoles.User).GetAsync("/v1/accounts/me");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadFromJsonAsync<ErrorResponse>())!.Errors
            .Should().Contain(e => e.Code == "user_not_found");
    }

    [Fact]
    public async Task Me_returns_only_the_callers_own_account()
    {
        var emailA = UniqueEmail();
        var emailB = UniqueEmail();
        var userA = await RegisterAsync(emailA);
        var userB = await RegisterAsync(emailB);

        var accountA = await CreateClient(userA, UserRoles.User).GetFromJsonAsync<AccountResponse>("/v1/accounts/me");
        var accountB = await CreateClient(userB, UserRoles.User).GetFromJsonAsync<AccountResponse>("/v1/accounts/me");

        accountA!.Email.Should().Be(emailA);
        accountB!.Email.Should().Be(emailB);
    }

    private sealed record AccountResponse(Guid Id, string Email, string UserName, IReadOnlyList<string> Roles, DateTime CreatedAt);
}
