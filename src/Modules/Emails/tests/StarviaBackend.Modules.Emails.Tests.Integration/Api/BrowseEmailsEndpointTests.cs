using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StarviaBackend.Modules.Emails.Application.Emails.Queries.BrowseEmails;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Emails.Tests.Integration.Api;

public sealed class BrowseEmailsEndpointTests : IAsyncLifetime
{
    private const string Route = "/v1/emails";

    private EmailsApiHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = await EmailsApiHost.StartAsync();
        _host.Dispatcher.QueryResult = query =>
        {
            var browse = (BrowseEmailsQuery)query;
            return new PagedResult<EmailDto>(
                [new EmailDto(Guid.NewGuid(), "Subject", "user@example.com", "<p>Body</p>", new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc))],
                browse.Page,
                browse.PageSize,
                1);
        };
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Anonymous_callers_are_rejected()
    {
        var response = await _host.CreateClient().GetAsync(Route);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        _host.Dispatcher.Queries.Should().BeEmpty();
    }

    [Fact]
    public async Task Signed_in_users_without_the_admin_role_are_forbidden()
    {
        var response = await _host.CreateClient(Guid.NewGuid(), UserRoles.User).GetAsync(Route);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        _host.Dispatcher.Queries.Should().BeEmpty();
    }

    [Fact]
    public async Task Admins_get_the_page_returned_by_the_query_handler()
    {
        var response = await _host.CreateClient(Guid.NewGuid(), UserRoles.Admin).GetAsync(Route);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<EmailDto>>();
        page.Should().NotBeNull();
        page!.TotalCount.Should().Be(1);
        var item = page.Items.Should().ContainSingle().Subject;
        item.Title.Should().Be("Subject");
        item.SentTo.Should().Be("user@example.com");
        item.Body.Should().Be("<p>Body</p>");
        item.SentAt.Should().Be(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));
    }

    [Fact]
    public async Task Paging_comes_from_the_query_string()
    {
        var response = await _host.CreateClient(Guid.NewGuid(), UserRoles.Admin).GetAsync($"{Route}?page=3&pageSize=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _host.Dispatcher.Queries.Should().ContainSingle()
            .Which.Should().Be(new BrowseEmailsQuery(3, 50));
    }

    [Fact]
    public async Task Missing_paging_defaults_to_the_first_page_of_twenty()
    {
        await _host.CreateClient(Guid.NewGuid(), UserRoles.Admin).GetAsync(Route);

        _host.Dispatcher.Queries.Should().ContainSingle()
            .Which.Should().Be(new BrowseEmailsQuery(1, 20));
    }

    [Fact]
    public async Task Out_of_range_paging_is_normalised_instead_of_rejected()
    {
        var response = await _host.CreateClient(Guid.NewGuid(), UserRoles.Admin).GetAsync($"{Route}?page=0&pageSize=500");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _host.Dispatcher.Queries.Should().ContainSingle()
            .Which.Should().Be(new BrowseEmailsQuery(1, 20));
    }

    [Fact]
    public async Task A_user_holding_several_roles_including_admin_is_allowed()
    {
        var response = await _host.CreateClient(Guid.NewGuid(), UserRoles.User, UserRoles.Admin).GetAsync(Route);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
