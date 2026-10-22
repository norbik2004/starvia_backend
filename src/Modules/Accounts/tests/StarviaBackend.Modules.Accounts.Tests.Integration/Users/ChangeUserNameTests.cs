using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Users;

public sealed class ChangeUserNameTests(AccountsApp app) : AccountsFlowTest(app)
{
    private const string Route = "/v1/accounts/change-user-name";

    [Fact]
    public async Task Changing_the_user_name_returns_the_user_id_and_persists_the_new_name()
    {
        var userId = await RegisterAsync();

        var response = await CreateClient(userId, UserRoles.User)
            .PutAsJsonAsync(Route, new { userName = "Newname" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<ChangeUserNameResponse>())!.UserId.Should().Be(userId);
        await WithDbAsync<AccountsWriteDbContext>(async db =>
            (await db.Users.SingleAsync(u => u.Id == userId)).UserName.Should().Be("Newname"));
    }

    [Fact]
    public async Task The_new_name_shows_up_on_the_account()
    {
        var userId = await RegisterAsync();
        var client = CreateClient(userId, UserRoles.User);

        await client.PutAsJsonAsync(Route, new { userName = "Another" });
        var account = await client.GetFromJsonAsync<AccountResponse>("/v1/accounts/me");

        account!.UserName.Should().Be("Another");
    }

    [Fact]
    public async Task The_email_is_left_untouched()
    {
        var email = UniqueEmail();
        var userId = await RegisterAsync(email);

        await CreateClient(userId, UserRoles.User).PutAsJsonAsync(Route, new { userName = "Newname" });

        await WithDbAsync<AccountsWriteDbContext>(async db =>
            (await db.Users.SingleAsync(u => u.Id == userId)).Email.Should().Be(email));
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("with1digit")]
    [InlineData("with space")]
    [InlineData("")]
    public async Task An_invalid_name_is_rejected_and_the_old_one_is_kept(string userName)
    {
        var email = UniqueEmail();
        var userId = await RegisterAsync(email);

        var response = await CreateClient(userId, UserRoles.User).PutAsJsonAsync(Route, new { userName });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await WithDbAsync<AccountsWriteDbContext>(async db =>
            (await db.Users.SingleAsync(u => u.Id == userId)).UserName.Should().Be(email));
    }

    [Fact]
    public async Task A_caller_without_an_identity_gets_401()
    {
        var response = await CreateAnonymousClient().PutAsJsonAsync(Route, new { userName = "Newname" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_token_for_an_account_that_does_not_exist_is_a_user_not_found_error()
    {
        var response = await CreateClient(Guid.NewGuid(), UserRoles.User)
            .PutAsJsonAsync(Route, new { userName = "Newname" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Errors.Should().Contain(e => e.Code == "user_not_found");
    }

    private sealed record ChangeUserNameResponse(Guid UserId);

    private sealed record AccountResponse(Guid Id, string Email, string UserName, IReadOnlyList<string> Roles, DateTime CreatedAt);
}
