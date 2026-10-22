using FluentAssertions;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.AddContentCalendar;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.RemoveContentCalendar;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.UpdateContentCalendar;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Tests.Unit.Fakes;

namespace StarviaBackend.Modules.Calendar.Tests.Unit.Handlers;

public sealed class AddContentCalendarHandlerTests
{
    private readonly InMemoryCalendarRepository _calendars = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task HandleAsync_StoresTheCalendarAndReturnsItsId()
    {
        // Arrange
        var handler = new AddContentCalendarHandler(_calendars);

        // Act
        var result = await handler.HandleAsync(new AddContentCalendarCommand(new AddContentCalendarRequest("Marketing"), _userId));

        // Assert
        var stored = _calendars.Calendars.Should().ContainSingle().Subject;
        result.contentCalendarId.Should().Be(stored.Id);
        stored.Name.Should().Be("Marketing");
    }

    [Fact]
    public async Task HandleAsync_WhenUserAlreadyHasACalendar_Throws()
    {
        // Arrange
        _calendars.Seed(_userId);
        var handler = new AddContentCalendarHandler(_calendars);

        // Act
        var act = () => handler.HandleAsync(new AddContentCalendarCommand(new AddContentCalendarRequest("Another"), _userId));

        // Assert
        var exception = await act.Should().ThrowAsync<CantCreateCalendarException>();
        exception.Which.Code.Should().Be("calendar_cannot_be_created");
        _calendars.Calendars.Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_AllowsADifferentUserToCreateTheirOwnCalendar()
    {
        // Arrange
        _calendars.Seed(Guid.NewGuid());
        var handler = new AddContentCalendarHandler(_calendars);

        // Act
        await handler.HandleAsync(new AddContentCalendarCommand(new AddContentCalendarRequest("Mine"), _userId));

        // Assert
        _calendars.Calendars.Should().HaveCount(2);
    }
}

public sealed class UpdateContentCalendarHandlerTests
{
    private readonly InMemoryCalendarRepository _calendars = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    private UpdateContentCalendarHandler CreateHandler() => new(_calendars);

    private static UpdateContentCalendarCommand Command(Guid calendarId, Guid userId, string name = "Renamed") =>
        new(new UpdateContentCalendarRequest(calendarId, name), userId);

    [Fact]
    public async Task HandleAsync_WhenCalendarDoesNotExist_Throws()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(Command(Guid.NewGuid(), _ownerId));

        // Assert
        await act.Should().ThrowAsync<CalendarNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotTheCreator_ThrowsAndKeepsTheName()
    {
        // Arrange
        var calendar = _calendars.Seed(_ownerId, "Original");

        // Act
        var act = () => CreateHandler().HandleAsync(Command(calendar.Id, Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        calendar.Name.Should().Be("Original");
        _calendars.Updated.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsTheCreator_RenamesAndPersistsTheCalendar()
    {
        // Arrange
        var calendar = _calendars.Seed(_ownerId, "Original");

        // Act
        var result = await CreateHandler().HandleAsync(Command(calendar.Id, _ownerId, "Renamed"));

        // Assert
        result.contentCalendarId.Should().Be(calendar.Id);
        calendar.Name.Should().Be("Renamed");
        _calendars.Updated.Should().ContainSingle().Which.Should().BeSameAs(calendar);
    }
}

public sealed class RemoveContentCalendarHandlerTests
{
    private readonly InMemoryCalendarRepository _calendars = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    private RemoveContentCalendarHandler CreateHandler() => new(_calendars);

    private static RemoveContentCalendarCommand Command(Guid calendarId, Guid userId) =>
        new(new RemoveContentCalendarRequest(calendarId), userId);

    [Fact]
    public async Task HandleAsync_WhenCalendarDoesNotExist_Throws()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(Command(Guid.NewGuid(), _ownerId));

        // Assert
        await act.Should().ThrowAsync<CalendarNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotTheCreator_ThrowsAndKeepsTheCalendar()
    {
        // Arrange
        var calendar = _calendars.Seed(_ownerId);

        // Act
        var act = () => CreateHandler().HandleAsync(Command(calendar.Id, Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _calendars.Calendars.Should().ContainSingle();
        _calendars.Removed.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsTheCreator_RemovesTheCalendar()
    {
        // Arrange
        var calendar = _calendars.Seed(_ownerId);

        // Act
        await CreateHandler().HandleAsync(Command(calendar.Id, _ownerId));

        // Assert
        _calendars.Calendars.Should().BeEmpty();
        _calendars.Removed.Should().ContainSingle().Which.Should().BeSameAs(calendar);
    }
}
