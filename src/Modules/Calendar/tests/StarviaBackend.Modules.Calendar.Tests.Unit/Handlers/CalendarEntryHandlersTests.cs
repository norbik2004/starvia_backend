using FluentAssertions;
using StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.RemoveCalendarEntry;
using StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.UpdateCalendarEntry;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Tests.Unit.Fakes;

namespace StarviaBackend.Modules.Calendar.Tests.Unit.Handlers;

public sealed class CalendarEntryHandlersTests
{
    private readonly InMemoryCalendarRepository _calendars = new();
    private readonly InMemoryCalendarEntryRepository _entries = new();
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly DateTimeOffset _original = new(2026, 12, 24, 10, 0, 0, TimeSpan.Zero);
    private readonly DateTimeOffset _updated = new(2027, 1, 2, 8, 0, 0, TimeSpan.Zero);

    private UpdateCalendarEntryHandler UpdateHandler(Guid? actingUserId = null, bool isAdmin = false) =>
        new(_entries, _calendars, TestAccess.For(actingUserId ?? _ownerId, isAdmin));

    private RemoveCalendarEntryHandler RemoveHandler(Guid? actingUserId = null, bool isAdmin = false) =>
        new(_entries, _calendars, TestAccess.For(actingUserId ?? _ownerId, isAdmin));

    private CalendarEntry SeedEntry(string? entryCreatedBy = null)
    {
        var calendar = _calendars.Seed(_ownerId);
        var entry = CalendarEntry.Create(calendar.Id, Guid.NewGuid(), _original);
        // Whoever added the entry (e.g. an admin) must not matter: the calendar owner decides.
        entry.CreatedBy = entryCreatedBy ?? _ownerId.ToString();
        _entries.Entries.Add(entry);
        return entry;
    }

    private UpdateCalendarEntryCommand UpdateCommand(Guid entryId, Guid userId) =>
        new(new UpdateCalendarEntryRequest(entryId, _updated), userId);

    private static RemoveCalendarEntryCommand RemoveCommand(Guid entryId, Guid userId) =>
        new(new RemoveCalendarEntryRequest(entryId), userId);

    [Fact]
    public async Task Update_WhenEntryDoesNotExist_Throws()
    {
        var act = () => UpdateHandler().HandleAsync(UpdateCommand(Guid.NewGuid(), _ownerId));

        await act.Should().ThrowAsync<CalendarEntryNotFoundException>();
    }

    [Fact]
    public async Task Update_WhenUserDoesNotOwnTheCalendar_ThrowsAndKeepsTheSchedule()
    {
        // Arrange
        var entry = SeedEntry();
        var stranger = Guid.NewGuid();

        // Act
        var act = () => UpdateHandler(stranger).HandleAsync(UpdateCommand(entry.Id, stranger));

        // Assert
        await act.Should().ThrowAsync<CalendarAccessDeniedException>();
        entry.ScheduledAt.Should().Be(_original);
    }

    [Fact]
    public async Task Update_WhenUserOwnsTheCalendarButDidNotCreateTheEntry_Succeeds()
    {
        // Arrange
        var entry = SeedEntry(entryCreatedBy: Guid.NewGuid().ToString());

        // Act
        await UpdateHandler().HandleAsync(UpdateCommand(entry.Id, _ownerId));

        // Assert
        entry.ScheduledAt.Should().Be(_updated);
    }

    [Fact]
    public async Task Update_WhenUserIsAnAdmin_ChangesSomeoneElsesEntry()
    {
        // Arrange
        var entry = SeedEntry();
        var admin = Guid.NewGuid();

        // Act
        await UpdateHandler(admin, isAdmin: true).HandleAsync(UpdateCommand(entry.Id, admin));

        // Assert
        entry.ScheduledAt.Should().Be(_updated);
    }

    [Fact]
    public async Task Remove_WhenEntryDoesNotExist_Throws()
    {
        var act = () => RemoveHandler().HandleAsync(RemoveCommand(Guid.NewGuid(), _ownerId));

        await act.Should().ThrowAsync<CalendarEntryNotFoundException>();
    }

    [Fact]
    public async Task Remove_WhenUserDoesNotOwnTheCalendar_ThrowsAndKeepsTheEntry()
    {
        // Arrange
        var entry = SeedEntry();
        var stranger = Guid.NewGuid();

        // Act
        var act = () => RemoveHandler(stranger).HandleAsync(RemoveCommand(entry.Id, stranger));

        // Assert
        await act.Should().ThrowAsync<CalendarAccessDeniedException>();
        _entries.Entries.Should().ContainSingle();
    }

    [Fact]
    public async Task Remove_WhenUserOwnsTheCalendar_RemovesTheEntry()
    {
        // Arrange
        var entry = SeedEntry();

        // Act
        await RemoveHandler().HandleAsync(RemoveCommand(entry.Id, _ownerId));

        // Assert
        _entries.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task Remove_WhenUserIsAnAdmin_RemovesSomeoneElsesEntry()
    {
        // Arrange
        var entry = SeedEntry();
        var admin = Guid.NewGuid();

        // Act
        await RemoveHandler(admin, isAdmin: true).HandleAsync(RemoveCommand(entry.Id, admin));

        // Assert
        _entries.Entries.Should().BeEmpty();
    }
}
