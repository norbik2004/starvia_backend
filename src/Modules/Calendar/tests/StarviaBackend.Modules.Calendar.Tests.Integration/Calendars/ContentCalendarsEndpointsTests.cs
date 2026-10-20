using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Calendar.Tests.Integration.Calendars;

public sealed class ContentCalendarsEndpointsTests(CalendarApp app) : CalendarIntegrationTest(app)
{
    // --- add / get ---------------------------------------------------------------------------

    [Fact]
    public async Task Add_calendar_returns_created_and_the_user_can_read_it_back()
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var calendarId = await CreateCalendarAsync(client, "Marketing");

        // Assert
        var response = await client.GetAsync("/v1/calendars");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var calendar = await response.Content.ReadFromJsonAsync<CalendarResponse>();
        calendar!.Id.Should().Be(calendarId);
        calendar.Name.Should().Be("Marketing");
    }

    [Fact]
    public async Task Add_second_calendar_for_the_same_user_returns_cannot_be_created()
    {
        // Arrange
        var client = CreateUserClient();
        await CreateCalendarAsync(client);

        // Act
        var response = await client.PostAsJsonAsync("/v1/calendars", new { name = "Another" });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "calendar_cannot_be_created");
    }

    [Fact]
    public async Task Different_users_can_each_have_a_calendar()
    {
        // Act
        var first = await CreateCalendarAsync(CreateUserClient());
        var second = await CreateCalendarAsync(CreateUserClient());

        // Assert
        first.Should().NotBe(second);
    }

    [Fact]
    public async Task Add_calendar_without_a_name_returns_bad_request()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/v1/calendars", new { name = "" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_calendar_without_identity_returns_unauthorized()
    {
        // Arrange
        var anonymous = App.CreateClient();

        // Act
        var response = await anonymous.PostAsJsonAsync("/v1/calendars", new { name = "Anonymous" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_calendar_when_the_user_has_none_returns_calendar_not_found()
    {
        // Act
        var response = await CreateUserClient().GetAsync("/v1/calendars");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "calendar_not_found");
    }

    [Fact]
    public async Task Get_calendar_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().GetAsync("/v1/calendars");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Users_only_see_their_own_calendar()
    {
        // Arrange
        var owner = CreateUserClient();
        var other = CreateUserClient();
        var ownerCalendar = await CreateCalendarAsync(owner, "Owner calendar");
        var otherCalendar = await CreateCalendarAsync(other, "Other calendar");

        // Act
        var seenByOwner = await owner.GetFromJsonAsync<CalendarResponse>("/v1/calendars");
        var seenByOther = await other.GetFromJsonAsync<CalendarResponse>("/v1/calendars");

        // Assert
        seenByOwner!.Id.Should().Be(ownerCalendar);
        seenByOther!.Id.Should().Be(otherCalendar);
    }

    // --- update ------------------------------------------------------------------------------

    [Fact]
    public async Task Update_calendar_renames_it()
    {
        // Arrange
        var client = CreateUserClient();
        var calendarId = await CreateCalendarAsync(client, "Old name");

        // Act
        var response = await client.PutAsJsonAsync("/v1/calendars", new { contentCalendarId = calendarId, name = "New name" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CalendarIdResponse>();
        result!.ContentCalendarId.Should().Be(calendarId);

        var calendar = await client.GetFromJsonAsync<CalendarResponse>("/v1/calendars");
        calendar!.Name.Should().Be("New name");
    }

    [Fact]
    public async Task Update_calendar_with_too_long_name_returns_bad_request()
    {
        // Arrange
        var client = CreateUserClient();
        var calendarId = await CreateCalendarAsync(client);

        // Act
        var response = await client.PutAsJsonAsync("/v1/calendars", new
        {
            contentCalendarId = calendarId,
            name = new string('n', 101),
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_unknown_calendar_returns_calendar_not_found()
    {
        // Act
        var response = await Client.PutAsJsonAsync("/v1/calendars", new { contentCalendarId = Guid.NewGuid(), name = "Name" });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "calendar_not_found");
    }

    // --- remove ------------------------------------------------------------------------------

    [Fact]
    public async Task Remove_calendar_deletes_it_and_lets_the_user_create_a_new_one()
    {
        // Arrange
        var client = CreateUserClient();
        var calendarId = await CreateCalendarAsync(client);

        // Act
        var response = await DeleteWithBodyAsync(client, "/v1/calendars", new { contentCalendarId = calendarId });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var afterwards = await client.GetAsync("/v1/calendars");
        await AssertErrorCodeAsync(afterwards, HttpStatusCode.BadRequest, "calendar_not_found");
        (await CreateCalendarAsync(client)).Should().NotBe(calendarId);
    }

    [Fact]
    public async Task Remove_unknown_calendar_returns_calendar_not_found()
    {
        // Act
        var response = await DeleteWithBodyAsync(Client, "/v1/calendars", new { contentCalendarId = Guid.NewGuid() });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "calendar_not_found");
    }

    [Fact]
    public async Task Remove_calendar_with_empty_id_returns_bad_request()
    {
        // Act
        var response = await DeleteWithBodyAsync(Client, "/v1/calendars", new { contentCalendarId = Guid.Empty });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Remove_calendar_also_deletes_its_entries()
    {
        // Arrange
        var client = CreateUserClient();
        var calendarId = await CreateCalendarAsync(client);
        var postId = Guid.NewGuid();
        var added = await client.PostAsJsonAsync("/v1/calendar-entries", new
        {
            calendarId,
            postId,
            scheduledAt = DateTimeOffset.UtcNow.AddDays(1),
        });
        added.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act
        var response = await DeleteWithBodyAsync(client, "/v1/calendars", new { contentCalendarId = calendarId });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var remaining = await WithDbAsync<CalendarWriteDbContext, int>(db =>
            db.Set<CalendarEntry>().CountAsync(e => e.ContentCalendarId == calendarId));
        remaining.Should().Be(0);
    }
}
