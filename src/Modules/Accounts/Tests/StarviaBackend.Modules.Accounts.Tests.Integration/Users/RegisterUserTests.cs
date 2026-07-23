using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;
using ModularMonolith.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Users;

public sealed class RegisterUserTests(AccountsApp app) : AccountsIntegrationTest(app)
{
    [Fact]
    public async Task Register_with_valid_data_returns_userId_and_persists_the_user()
    {
        var email = UniqueEmail();
        var response = await Client.PostAsJsonAsync(
            "/v1/accounts/register",
            new { email, password = "Passw0rd!", userName = email });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        body!.UserId.Should().NotBeEmpty();

        await WithDbAsync<AccountsWriteDbContext>(async db =>
            (await db.Users.AnyAsync(u => u.Email == email)).Should().BeTrue());
    }

    [Fact]
    public async Task Register_with_duplicate_email_returns_bad_request()
    {
        var email = UniqueEmail();
        var request = new { email, password = "Passw0rd!", userName = email };

        (await Client.PostAsJsonAsync("/v1/accounts/register", request))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await Client.PostAsJsonAsync("/v1/accounts/register", request);

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await second.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Errors.Should().Contain(e => e.Code == "email_already_in_use");
    }

    [Fact]
    public async Task Register_with_invalid_email_returns_validation_error()
    {
        var response = await Client.PostAsJsonAsync(
            "/v1/accounts/register",
            new { email = "not-an-email", password = "Passw0rd!", userName = "x" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";

    private sealed record RegisterUserResponse(Guid UserId);
}
