using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;
using StarviaBackend.Modules.Calendar.Core.Calendars.Enums;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Calendar.Tests.Integration.Calendars;

public sealed class CalendarEntriesEndpointsTests(CalendarApp app) : CalendarIntegrationTest(app)
{
    private static readonly DateTimeOffset ScheduledAt = new(2027, 3, 14, 9, 30, 0, TimeSpan.Zero);

    private static object EntryRequest(Guid calendarId, Guid postId) => new { calendarId, postId, scheduledAt = ScheduledAt };

    [Fact]
    public async Task Add_entry_returns_created_and_stores_a_scheduled_entry()
    {
        // Arrange
        var client = CreateUserClient();
        var calendarId = await CreateCalendarAsync(client);
        var postId = Guid.NewGuid();

        // Act
        var response = await client.PostAsJsonAsync("/v1/calendar-entries", EntryRequest(calendarId, postId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CalendarEntryIdResponse>();

        var entry = await WithDbAsync<CalendarWriteDbContext, CalendarEntry>(db =>
            db.Set<CalendarEntry>().SingleAsync(e => e.Id == created!.CalendarEntryId));
        entry.ContentCalendarId.Should().Be(calendarId);
        entry.PostId.Should().Be(postId);
        entry.ScheduledAt.Should().Be(ScheduledAt);
        entry.Status.Should().Be(CalendarEntryStatus.Scheduled);
    }

    [Fact]
    public async Task Add_several_entries_to_the_same_calendar()
    {
        // Arrange
        var client = CreateUserClient();
        var calendarId = await CreateCalendarAsync(client);

        // Act
        var first = await client.PostAsJsonAsync("/v1/calendar-entries", EntryRequest(calendarId, Guid.NewGuid()));
        var second = await client.PostAsJsonAsync("/v1/calendar-entries", EntryRequest(calendarId, Guid.NewGuid()));

        // Assert
        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.Created);
        var count = await WithDbAsync<CalendarWriteDbContext, int>(db =>
            db.Set<CalendarEntry>().CountAsync(e => e.ContentCalendarId == calendarId));
        count.Should().Be(2);
    }

    [Fact]
    public async Task Add_entry_to_unknown_calendar_returns_calendar_not_found()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/v1/calendar-entries", EntryRequest(Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "calendar_not_found");
    }

    [Fact]
    public async Task Add_entry_to_someone_elses_calendar_returns_calendar_not_owned()
    {
        // Arrange
        var calendarId = await CreateCalendarAsync(CreateUserClient());
        var stranger = CreateUserClient();

        // Act
        var response = await stranger.PostAsJsonAsync("/v1/calendar-entries", EntryRequest(calendarId, Guid.NewGuid()));

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "calendar_not_owned");
        var count = await WithDbAsync<CalendarWriteDbContext, int>(db =>
            db.Set<CalendarEntry>().CountAsync(e => e.ContentCalendarId == calendarId));
        count.Should().Be(0);
    }

    [Fact]
    public async Task Add_entry_for_unknown_post_returns_posts_integration_failure()
    {
        // Arrange
        var client = CreateUserClient();
        var calendarId = await CreateCalendarAsync(client);
        var postId = Guid.NewGuid();
        App.Posts.UnknownPostIds.Add(postId);

        // Act
        var response = await client.PostAsJsonAsync("/v1/calendar-entries", EntryRequest(calendarId, postId));

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "posts_integration_failed");
        var count = await WithDbAsync<CalendarWriteDbContext, int>(db =>
            db.Set<CalendarEntry>().CountAsync(e => e.ContentCalendarId == calendarId));
        count.Should().Be(0);
    }

    [Fact]
    public async Task Add_entry_for_someone_elses_post_returns_post_not_owned()
    {
        // Arrange
        var client = CreateUserClient();
        var calendarId = await CreateCalendarAsync(client);
        var postId = Guid.NewGuid();
        App.Posts.ForeignPostIds.Add(postId);

        // Act
        var response = await client.PostAsJsonAsync("/v1/calendar-entries", EntryRequest(calendarId, postId));

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "post_not_owned");
        var count = await WithDbAsync<CalendarWriteDbContext, int>(db =>
            db.Set<CalendarEntry>().CountAsync(e => e.ContentCalendarId == calendarId));
        count.Should().Be(0);
    }

    [Fact]
    public async Task Add_entry_with_empty_ids_returns_bad_request()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/v1/calendar-entries", EntryRequest(Guid.Empty, Guid.Empty));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_entry_without_scheduled_time_returns_bad_request()
    {
        // Arrange
        var client = CreateUserClient();
        var calendarId = await CreateCalendarAsync(client);

        // Act
        var response = await client.PostAsJsonAsync("/v1/calendar-entries", new { calendarId, postId = Guid.NewGuid() });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_entry_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient()
            .PostAsJsonAsync("/v1/calendar-entries", EntryRequest(Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
