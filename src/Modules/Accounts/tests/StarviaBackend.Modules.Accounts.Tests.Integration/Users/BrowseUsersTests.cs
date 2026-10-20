using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StarviaBackend.Modules.Accounts.Application.Users.Queries.BrowseUsers;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Users;

public sealed class BrowseUsersTests(AccountsApp app) : AccountsFlowTest(app)
{
    private const string Route = "/v1/accounts";

    private HttpClient Admin => CreateClient(Guid.NewGuid(), UserRoles.Admin);

    [Fact]
    public async Task Anonymous_callers_get_401()
    {
        (await CreateAnonymousClient().GetAsync(Route)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Regular_users_get_403()
    {
        var response = await CreateClient(Guid.NewGuid(), UserRoles.User).GetAsync(Route);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Registered_users_are_listed_with_their_account_details()
    {
        var email = UniqueEmail();
        var userId = await RegisterAsync(email);

        var page = await Admin.GetFromJsonAsync<PagedResult<UserDto>>($"{Route}?pageSize=100");

        var user = page!.Items.Should().ContainSingle(u => u.Id == userId).Subject;
        user.Email.Should().Be(email);
        user.UserName.Should().Be(email);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
        page.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Users_are_ordered_by_creation_time()
    {
        var first = await RegisterAsync();
        await Task.Delay(20);
        var second = await RegisterAsync();

        var page = await Admin.GetFromJsonAsync<PagedResult<UserDto>>($"{Route}?pageSize=100");

        var ids = page!.Items.Select(u => u.Id).ToList();
        ids.IndexOf(first).Should().BeGreaterThanOrEqualTo(0);
        ids.IndexOf(first).Should().BeLessThan(ids.IndexOf(second));
        page.Items.Select(u => u.CreatedAt).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Paging_limits_the_page_and_reports_the_totals()
    {
        await RegisterAsync();
        await RegisterAsync();

        var page = await Admin.GetFromJsonAsync<PagedResult<UserDto>>($"{Route}?page=1&pageSize=1");

        page!.Items.Should().HaveCount(1);
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(1);
        page.TotalCount.Should().BeGreaterThanOrEqualTo(2);
        page.TotalPages.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Different_pages_return_different_users()
    {
        await RegisterAsync();
        await RegisterAsync();

        var first = await Admin.GetFromJsonAsync<PagedResult<UserDto>>($"{Route}?page=1&pageSize=1");
        var second = await Admin.GetFromJsonAsync<PagedResult<UserDto>>($"{Route}?page=2&pageSize=1");

        first!.Items.Single().Id.Should().NotBe(second!.Items.Single().Id);
    }

    [Fact]
    public async Task Out_of_range_paging_is_normalised_instead_of_rejected()
    {
        var response = await Admin.GetAsync($"{Route}?page=0&pageSize=1000");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<UserDto>>();
        page!.Page.Should().Be(1);
        page.PageSize.Should().Be(20);
    }
}
