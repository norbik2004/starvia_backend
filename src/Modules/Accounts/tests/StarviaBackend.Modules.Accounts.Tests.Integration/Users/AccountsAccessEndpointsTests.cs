using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Accounts.Application.Users.Queries.BrowseUsers;
using StarviaBackend.Modules.Accounts.Application.Users.Queries.GetAccount;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Exceptions;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Users;

/// <summary>Who may do what with accounts: an account's owner and admins for single accounts, admins only for the user list.</summary>
public sealed class AccountsAccessEndpointsTests(AccountsApp app) : AccountsFlowTest(app)
{
    private HttpClient Admin => CreateClient(Guid.NewGuid(), UserRoles.Admin);

    private HttpClient UserClient(Guid userId) => CreateClient(userId, UserRoles.User);

    private static async Task<string> CodeOfAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ErrorResponse>())!.Errors.Single().Code;

    // ---- account by id ---------------------------------------------------------------------------

    [Fact]
    public async Task Get_account_by_id_returns_it_to_its_owner_and_to_an_admin()
    {
        var email = UniqueEmail();
        var userId = await RegisterAsync(email);

        var asOwner = await UserClient(userId).GetFromJsonAsync<AccountDto>($"/v1/accounts/{userId}");
        var asAdmin = await Admin.GetFromJsonAsync<AccountDto>($"/v1/accounts/{userId}");

        asOwner!.Id.Should().Be(userId);
        asOwner.Email.Should().Be(email);
        asAdmin!.Id.Should().Be(userId);
    }

    [Fact]
    public async Task Get_account_by_id_is_forbidden_for_another_user()
    {
        var userId = await RegisterAsync();

        var response = await UserClient(Guid.NewGuid()).GetAsync($"/v1/accounts/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await CodeOfAsync(response)).Should().Be("account_access_denied");
    }

    [Fact]
    public async Task Get_account_by_id_does_not_reveal_whether_an_account_exists()
    {
        var existing = await RegisterAsync();
        var stranger = UserClient(Guid.NewGuid());

        var forExisting = await stranger.GetAsync($"/v1/accounts/{existing}");
        var forMissing = await stranger.GetAsync($"/v1/accounts/{Guid.NewGuid()}");

        forExisting.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        forMissing.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Get_account_by_id_for_a_missing_account_is_reported_to_an_admin()
    {
        var response = await Admin.GetAsync($"/v1/accounts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await CodeOfAsync(response)).Should().Be("user_not_found");
    }

    [Fact]
    public async Task Get_account_by_id_requires_authentication()
    {
        var response = await CreateAnonymousClient().GetAsync($"/v1/accounts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task The_me_route_still_returns_the_callers_own_account()
    {
        var userId = await RegisterAsync();

        var account = await UserClient(userId).GetFromJsonAsync<AccountDto>("/v1/accounts/me");

        account!.Id.Should().Be(userId);
    }

    // ---- change user name ------------------------------------------------------------------------

    [Fact]
    public async Task Changing_the_user_name_acts_on_the_callers_own_account_only()
    {
        var caller = await RegisterAsync();
        var other = await RegisterAsync();

        var response = await UserClient(caller).PutAsJsonAsync("/v1/accounts/change-user-name", new { userName = "Mine" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await WithDbAsync<AccountsWriteDbContext>(async db =>
        {
            (await db.Users.SingleAsync(u => u.Id == caller)).UserName.Should().Be("Mine");
            (await db.Users.SingleAsync(u => u.Id == other)).UserName.Should().NotBe("Mine");
        });
    }

    // ---- browse users ----------------------------------------------------------------------------

    [Fact]
    public async Task Browsing_users_is_forbidden_for_a_plain_user_with_an_error_code()
    {
        var response = await UserClient(Guid.NewGuid()).GetAsync("/v1/accounts");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await CodeOfAsync(response)).Should().Be("account_access_denied");
    }

    [Fact]
    public async Task Search_matches_the_email_case_insensitively()
    {
        var token = Guid.NewGuid().ToString("N");
        var matching = await RegisterAsync($"Find-{token}@example.com");
        await RegisterAsync();

        var page = await Admin.GetFromJsonAsync<PagedResult<UserDto>>($"/v1/accounts?search={token.ToUpperInvariant()}");

        page!.Items.Should().ContainSingle().Which.Id.Should().Be(matching);
        page.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Search_matches_the_user_name_too()
    {
        // User names are letters only and at most 25 long, so build a unique one from letters.
        var token = new string(Guid.NewGuid().ToString("N").Take(16).Select(c => (char)('a' + c % 26)).ToArray());
        var userId = await RegisterAsync();
        (await UserClient(userId).PutAsJsonAsync("/v1/accounts/change-user-name", new { userName = $"Nick{token}" }))
            .EnsureSuccessStatusCode();

        var page = await Admin.GetFromJsonAsync<PagedResult<UserDto>>($"/v1/accounts?search=NICK{token}");

        page!.Items.Should().ContainSingle().Which.Id.Should().Be(userId);
    }

    [Fact]
    public async Task Search_treats_wildcards_literally()
    {
        var token = Guid.NewGuid().ToString("N");
        var underscore = await RegisterAsync($"a_{token}@example.com");
        await RegisterAsync($"aX{token}@example.com");

        var withUnderscore = await Admin.GetFromJsonAsync<PagedResult<UserDto>>($"/v1/accounts?search=a_{token}");
        var withPercent = await Admin.GetFromJsonAsync<PagedResult<UserDto>>($"/v1/accounts?search=%25{token}");

        withUnderscore!.Items.Should().ContainSingle().Which.Id.Should().Be(underscore);
        withPercent!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Users_can_be_sorted_by_email_in_both_directions()
    {
        var token = Guid.NewGuid().ToString("N");
        await RegisterAsync($"{token}-b@example.com");
        await RegisterAsync($"{token}-c@example.com");
        await RegisterAsync($"{token}-a@example.com");

        var ascending = await Admin.GetFromJsonAsync<PagedResult<UserDto>>($"/v1/accounts?search={token}&sortBy=Email");
        var descending = await Admin.GetFromJsonAsync<PagedResult<UserDto>>($"/v1/accounts?search={token}&sortBy=Email&descending=true");

        ascending!.Items.Select(u => u.Email).Should().ContainInConsecutiveOrder(
            $"{token}-a@example.com", $"{token}-b@example.com", $"{token}-c@example.com");
        descending!.Items.Select(u => u.Email).Should().ContainInConsecutiveOrder(
            $"{token}-c@example.com", $"{token}-b@example.com", $"{token}-a@example.com");
    }

    [Fact]
    public async Task Users_default_to_newest_last_and_can_be_listed_newest_first()
    {
        var token = Guid.NewGuid().ToString("N");
        var first = await RegisterAsync($"{token}-1@example.com");
        await Task.Delay(20);
        var second = await RegisterAsync($"{token}-2@example.com");

        var newestFirst = await Admin.GetFromJsonAsync<PagedResult<UserDto>>($"/v1/accounts?search={token}&descending=true");

        newestFirst!.Items.Select(u => u.Id).Should().ContainInConsecutiveOrder(second, first);
    }

    [Fact]
    public async Task A_page_past_the_end_is_empty_but_still_reports_the_total()
    {
        var token = Guid.NewGuid().ToString("N");
        await RegisterAsync($"{token}-1@example.com");
        await RegisterAsync($"{token}-2@example.com");

        var page = await Admin.GetFromJsonAsync<PagedResult<UserDto>>($"/v1/accounts?search={token}&page=5&pageSize=10");

        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(2);
        page.Page.Should().Be(5);
    }

    [Fact]
    public async Task The_last_login_is_listed_once_the_user_has_signed_in()
    {
        var (userId, email) = await RegisterAndConfirmAsync();
        var never = await RegisterAsync();
        (await Client.PostAsJsonAsync("/v1/accounts/sign-in", new { email, password = Password }))
            .EnsureSuccessStatusCode();

        var page = await Admin.GetFromJsonAsync<PagedResult<UserDto>>("/v1/accounts?pageSize=100");

        page!.Items.Single(u => u.Id == userId).LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
        page.Items.Single(u => u.Id == never).LastLoginAt.Should().BeNull();
    }
}