using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;
using StarviaBackend.Modules.Calendar.Core.Calendars.Enums;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Calendar.Tests.Integration.Calendars;

/// <summary>Who may do what with a calendar and its entries: the creator and admins, nobody else.</summary>
public sealed class CalendarAccessEndpointsTests(CalendarApp app) : CalendarIntegrationTest(app)
{
    private static readonly DateTimeOffset ScheduledAt = new(2027, 3, 14, 9, 30, 0, TimeSpan.Zero);

    private sealed record CalendarDetails(Guid Id, string Name, List<EntryDetails> CalendarEntries);

    private sealed record EntryDetails(Guid Id, Guid PostId, DateTimeOffset ScheduledAt, CalendarEntryStatus Status);

    private sealed record EntriesPage(List<EntryDetails> Items, int Page, int PageSize, int TotalCount);

    private static async Task<Guid> AddEntryAsync(HttpClient client, Guid calendarId, DateTimeOffset? at = null)
    {
        var response = await client.PostAsJsonAsync(
            "/v1/calendar-entries",
            new { calendarId, postId = Guid.NewGuid(), scheduledAt = at ?? ScheduledAt });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CalendarEntryIdResponse>())!.CalendarEntryId;
    }

    // ---- calendar by id -------------------------------------------------------------------------

    [Fact]
    public async Task Get_calendar_by_id_returns_it_to_the_owner_and_to_an_admin()
    {
        // Arrange
        var owner = CreateUserClient();
        var calendarId = await CreateCalendarAsync(owner, "Mine");
        await AddEntryAsync(owner, calendarId);

        // Act
        var asOwner = await owner.GetFromJsonAsync<CalendarDetails>($"/v1/calendars/{calendarId}");
        var asAdmin = await CreateAdminClient().GetFromJsonAsync<CalendarDetails>($"/v1/calendars/{calendarId}");

        // Assert
        asOwner!.Name.Should().Be("Mine");
        asOwner.CalendarEntries.Should().HaveCount(1);
        asAdmin!.Id.Should().Be(calendarId);
    }

    [Fact]
    public async Task Get_calendar_by_id_of_someone_else_returns_forbidden()
    {
        // Arrange
        var calendarId = await CreateCalendarAsync(CreateUserClient());

        // Act
        var response = await CreateUserClient().GetAsync($"/v1/calendars/{calendarId}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "calendar_access_denied");
    }

    [Fact]
    public async Task Get_unknown_calendar_by_id_returns_calendar_not_found()
    {
        // Act
        var response = await CreateUserClient().GetAsync($"/v1/calendars/{Guid.NewGuid()}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "calendar_not_found");
    }

    [Fact]
    public async Task Get_calendar_by_id_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().GetAsync($"/v1/calendars/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ---- update / remove calendar -------------------------------------------------------------

    [Fact]
    public async Task Update_someone_elses_calendar_returns_forbidden_and_keeps_the_name()
    {
        // Arrange
        var calendarId = await CreateCalendarAsync(CreateUserClient(), "Original");

        // Act
        var response = await CreateUserClient().PutAsJsonAsync("/v1/calendars", new { contentCalendarId = calendarId, name = "Hacked" });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "calendar_access_denied");
        var calendar = await WithDbAsync<CalendarWriteDbContext, ContentCalendar>(db =>
            db.Set<ContentCalendar>().SingleAsync(c => c.Id == calendarId));
        calendar.Name.Should().Be("Original");
    }

    [Fact]
    public async Task Update_someone_elses_calendar_as_admin_succeeds()
    {
        // Arrange
        var calendarId = await CreateCalendarAsync(CreateUserClient(), "Original");

        // Act
        var response = await CreateAdminClient().PutAsJsonAsync("/v1/calendars", new { contentCalendarId = calendarId, name = "By admin" });

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var calendar = await WithDbAsync<CalendarWriteDbContext, ContentCalendar>(db =>
            db.Set<ContentCalendar>().SingleAsync(c => c.Id == calendarId));
        calendar.Name.Should().Be("By admin");
    }

    [Fact]
    public async Task Remove_someone_elses_calendar_returns_forbidden_and_keeps_it()
    {
        // Arrange
        var calendarId = await CreateCalendarAsync(CreateUserClient());

        // Act
        var response = await DeleteWithBodyAsync(CreateUserClient(), "/v1/calendars", new { contentCalendarId = calendarId });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "calendar_access_denied");
        var exists = await WithDbAsync<CalendarWriteDbContext, bool>(db =>
            db.Set<ContentCalendar>().AnyAsync(c => c.Id == calendarId));
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Remove_someone_elses_calendar_as_admin_succeeds()
    {
        // Arrange
        var calendarId = await CreateCalendarAsync(CreateUserClient());

        // Act
        var response = await DeleteWithBodyAsync(CreateAdminClient(), "/v1/calendars", new { contentCalendarId = calendarId });

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var exists = await WithDbAsync<CalendarWriteDbContext, bool>(db =>
            db.Set<ContentCalendar>().AnyAsync(c => c.Id == calendarId));
        exists.Should().BeFalse();
    }

    // ---- update / remove entry ------------------------------------------------------------------

    [Fact]
    public async Task Update_entry_of_someone_elses_calendar_returns_forbidden_and_keeps_the_schedule()
    {
        // Arrange
        var owner = CreateUserClient();
        var entryId = await AddEntryAsync(owner, await CreateCalendarAsync(owner));

        // Act
        var response = await CreateUserClient().PutAsJsonAsync(
            "/v1/calendar-entries", new { calendarEntryId = entryId, scheduledAt = ScheduledAt.AddDays(5) });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "calendar_access_denied");
        var entry = await WithDbAsync<CalendarWriteDbContext, CalendarEntry>(db =>
            db.Set<CalendarEntry>().SingleAsync(e => e.Id == entryId));
        entry.ScheduledAt.Should().Be(ScheduledAt);
    }

    [Fact]
    public async Task Update_entry_as_owner_and_as_admin_changes_the_schedule()
    {
        // Arrange
        var owner = CreateUserClient();
        var entryId = await AddEntryAsync(owner, await CreateCalendarAsync(owner));

        // Act
        var asOwner = await owner.PutAsJsonAsync(
            "/v1/calendar-entries", new { calendarEntryId = entryId, scheduledAt = ScheduledAt.AddDays(1) });
        var asAdmin = await CreateAdminClient().PutAsJsonAsync(
            "/v1/calendar-entries", new { calendarEntryId = entryId, scheduledAt = ScheduledAt.AddDays(2) });

        // Assert
        asOwner.IsSuccessStatusCode.Should().BeTrue();
        asAdmin.IsSuccessStatusCode.Should().BeTrue();
        var entry = await WithDbAsync<CalendarWriteDbContext, CalendarEntry>(db =>
            db.Set<CalendarEntry>().SingleAsync(e => e.Id == entryId));
        entry.ScheduledAt.Should().Be(ScheduledAt.AddDays(2));
    }

    [Fact]
    public async Task Entry_added_by_an_admin_stays_manageable_by_the_calendar_owner()
    {
        // Arrange
        var owner = CreateUserClient();
        var calendarId = await CreateCalendarAsync(owner);
        var entryId = await AddEntryAsync(CreateAdminClient(), calendarId);

        // Act
        var response = await owner.PutAsJsonAsync(
            "/v1/calendar-entries", new { calendarEntryId = entryId, scheduledAt = ScheduledAt.AddDays(1) });

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Remove_entry_of_someone_elses_calendar_returns_forbidden_and_keeps_it()
    {
        // Arrange
        var owner = CreateUserClient();
        var entryId = await AddEntryAsync(owner, await CreateCalendarAsync(owner));

        // Act
        var response = await DeleteWithBodyAsync(CreateUserClient(), "/v1/calendar-entries", new { calendarEntryId = entryId });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "calendar_access_denied");
        var exists = await WithDbAsync<CalendarWriteDbContext, bool>(db =>
            db.Set<CalendarEntry>().AnyAsync(e => e.Id == entryId));
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Remove_entry_as_admin_deletes_it()
    {
        // Arrange
        var owner = CreateUserClient();
        var entryId = await AddEntryAsync(owner, await CreateCalendarAsync(owner));

        // Act
        var response = await DeleteWithBodyAsync(CreateAdminClient(), "/v1/calendar-entries", new { calendarEntryId = entryId });

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var exists = await WithDbAsync<CalendarWriteDbContext, bool>(db =>
            db.Set<CalendarEntry>().AnyAsync(e => e.Id == entryId));
        exists.Should().BeFalse();
    }

    // ---- browse entries --------------------------------------------------------------------------

    [Fact]
    public async Task Browse_entries_returns_them_soonest_first_with_paging_info()
    {
        // Arrange
        var owner = CreateUserClient();
        var calendarId = await CreateCalendarAsync(owner);
        var later = await AddEntryAsync(owner, calendarId, ScheduledAt.AddDays(3));
        var sooner = await AddEntryAsync(owner, calendarId, ScheduledAt.AddDays(1));
        var middle = await AddEntryAsync(owner, calendarId, ScheduledAt.AddDays(2));

        // Act
        var all = await owner.GetFromJsonAsync<EntriesPage>($"/v1/calendar-entries?calendarId={calendarId}");
        var descending = await owner.GetFromJsonAsync<EntriesPage>($"/v1/calendar-entries?calendarId={calendarId}&descending=true");
        var secondPage = await owner.GetFromJsonAsync<EntriesPage>($"/v1/calendar-entries?calendarId={calendarId}&page=2&pageSize=2");

        // Assert
        all!.Items.Select(e => e.Id).Should().Equal(sooner, middle, later);
        all.TotalCount.Should().Be(3);
        descending!.Items.Select(e => e.Id).Should().Equal(later, middle, sooner);
        secondPage!.Items.Select(e => e.Id).Should().Equal(later);
        secondPage.TotalCount.Should().Be(3);
        secondPage.Page.Should().Be(2);
        secondPage.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task Browse_entries_filters_by_time_window()
    {
        // Arrange
        var owner = CreateUserClient();
        var calendarId = await CreateCalendarAsync(owner);
        await AddEntryAsync(owner, calendarId, ScheduledAt.AddDays(1));
        var inside = await AddEntryAsync(owner, calendarId, ScheduledAt.AddDays(5));
        await AddEntryAsync(owner, calendarId, ScheduledAt.AddDays(10));
        var from = Uri.EscapeDataString(ScheduledAt.AddDays(3).ToString("O"));
        var to = Uri.EscapeDataString(ScheduledAt.AddDays(7).ToString("O"));

        // Act
        var page = await owner.GetFromJsonAsync<EntriesPage>($"/v1/calendar-entries?calendarId={calendarId}&from={from}&to={to}");

        // Assert
        page!.Items.Select(e => e.Id).Should().Equal(inside);
        page.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Browse_entries_filters_by_status()
    {
        // Arrange
        var owner = CreateUserClient();
        var calendarId = await CreateCalendarAsync(owner);
        await AddEntryAsync(owner, calendarId);

        // Act
        var scheduled = await owner.GetFromJsonAsync<EntriesPage>($"/v1/calendar-entries?calendarId={calendarId}&status=Scheduled");
        var published = await owner.GetFromJsonAsync<EntriesPage>($"/v1/calendar-entries?calendarId={calendarId}&status=Published");

        // Assert
        scheduled!.TotalCount.Should().Be(1);
        published!.Items.Should().BeEmpty();
        published.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Browse_entries_page_past_the_end_returns_empty_items_but_the_total()
    {
        // Arrange
        var owner = CreateUserClient();
        var calendarId = await CreateCalendarAsync(owner);
        await AddEntryAsync(owner, calendarId);

        // Act
        var page = await owner.GetFromJsonAsync<EntriesPage>($"/v1/calendar-entries?calendarId={calendarId}&page=50");

        // Assert
        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Browse_entries_of_someone_elses_calendar_returns_forbidden_but_an_admin_may()
    {
        // Arrange
        var owner = CreateUserClient();
        var calendarId = await CreateCalendarAsync(owner);
        await AddEntryAsync(owner, calendarId);

        // Act
        var asStranger = await CreateUserClient().GetAsync($"/v1/calendar-entries?calendarId={calendarId}");
        var asAdmin = await CreateAdminClient().GetFromJsonAsync<EntriesPage>($"/v1/calendar-entries?calendarId={calendarId}");

        // Assert
        await AssertErrorCodeAsync(asStranger, HttpStatusCode.Forbidden, "calendar_access_denied");
        asAdmin!.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Browse_entries_of_unknown_calendar_returns_calendar_not_found()
    {
        // Act
        var response = await CreateUserClient().GetAsync($"/v1/calendar-entries?calendarId={Guid.NewGuid()}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "calendar_not_found");
    }

    [Fact]
    public async Task Browse_entries_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().GetAsync($"/v1/calendar-entries?calendarId={Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
