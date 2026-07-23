using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ModularMonolith.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Users;

public sealed class SignInAndAccountTests(AccountsApp app) : AccountsIntegrationTest(app)
{
    [Fact]
    public async Task SignIn_with_valid_credentials_returns_a_token()
    {
        var email = UniqueEmail();
        await Register(email);

        var response = await Client.PostAsJsonAsync(
            "/v1/accounts/sign-in",
            new { email, password = "Passw0rd!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadFromJsonAsync<JsonWebToken>();
        token!.AccessToken.Should().NotBeNullOrWhiteSpace();
        token.Email.Should().Be(email);
        token.Roles.Should().Contain(UserRoles.User);
    }

    [Fact]
    public async Task SignIn_with_wrong_password_returns_bad_request()
    {
        var email = UniqueEmail();
        await Register(email);

        var response = await Client.PostAsJsonAsync(
            "/v1/accounts/sign-in",
            new { email, password = "wrong-password" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_me_returns_the_current_account()
    {
        var email = UniqueEmail();
        var userId = await Register(email);

        var client = CreateClient(userId, UserRoles.User);
        var response = await client.GetAsync("/v1/accounts/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>();
        account!.Email.Should().Be(email);
        account.Roles.Should().Contain(UserRoles.User);
    }

    [Fact]
    public async Task Browse_users_requires_admin_role()
    {
        var asUser = CreateClient(Guid.NewGuid(), UserRoles.User);
        (await asUser.GetAsync("/v1/accounts")).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var asAdmin = CreateClient(Guid.NewGuid(), UserRoles.Admin);
        (await asAdmin.GetAsync("/v1/accounts")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<Guid> Register(string email)
    {
        var response = await Client.PostAsJsonAsync(
            "/v1/accounts/register",
            new { email, password = "Passw0rd!", userName = email });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        return body!.UserId;
    }

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";

    private sealed record RegisterUserResponse(Guid UserId);

    private sealed record AccountResponse(Guid Id, string Email, string UserName, IReadOnlyList<string> Roles, DateTime CreatedAt);
}
