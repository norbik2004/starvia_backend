using FluentAssertions;
using StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.AddCalendarEntry;
using StarviaBackend.Modules.Calendar.Core.Calendars.Enums;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Tests.Unit.Fakes;
using StarviaBackend.Modules.Posts.Integrations;

namespace StarviaBackend.Modules.Calendar.Tests.Unit.Handlers;

public sealed class AddCalendarEntryHandlerTests
{
    private readonly InMemoryCalendarRepository _calendars = new();
    private readonly InMemoryCalendarEntryRepository _entries = new();
    private readonly FakePostsClient _posts = new();
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly DateTimeOffset _scheduledAt = new(2026, 12, 24, 10, 30, 0, TimeSpan.Zero);

    private AddCalendarEntryHandler CreateHandler() => new(_calendars, _entries, _posts);

    private AddCalendarEntryCommand Command(Guid calendarId, Guid postId, Guid userId) =>
        new(new AddCalendarEntryRequest(calendarId, postId, _scheduledAt), userId);

    [Fact]
    public async Task HandleAsync_WhenCalendarDoesNotExist_ThrowsWithoutCallingPosts()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(Command(Guid.NewGuid(), Guid.NewGuid(), _ownerId));

        // Assert
        var exception = await act.Should().ThrowAsync<CalendarNotFoundException>();
        exception.Which.Code.Should().Be("calendar_not_found");
        _posts.Calls.Should().BeEmpty();
        _entries.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenCalendarBelongsToAnotherUser_ThrowsWithoutCallingPosts()
    {
        // Arrange
        var calendar = _calendars.Seed(_ownerId);

        // Act
        var act = () => CreateHandler().HandleAsync(Command(calendar.Id, Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        var exception = await act.Should().ThrowAsync<CalendarNotOwnedException>();
        exception.Which.Code.Should().Be("calendar_not_owned");
        _posts.Calls.Should().BeEmpty();
        _entries.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenPostDoesNotExist_PropagatesTheFailureAndStoresNothing()
    {
        // Arrange
        var calendar = _calendars.Seed(_ownerId);
        var postId = Guid.NewGuid();
        _posts.UnknownPostIds.Add(postId);

        // Act
        var act = () => CreateHandler().HandleAsync(Command(calendar.Id, postId, _ownerId));

        // Assert
        await act.Should().ThrowAsync<PostsIntegrationException>();
        _entries.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenPostBelongsToAnotherUser_PropagatesTheFailureAndStoresNothing()
    {
        // Arrange
        var calendar = _calendars.Seed(_ownerId);
        var postId = Guid.NewGuid();
        _posts.ForeignPostIds.Add(postId);

        // Act
        var act = () => CreateHandler().HandleAsync(Command(calendar.Id, postId, _ownerId));

        // Assert
        await act.Should().ThrowAsync<PostsIntegrationNotOwnerException>();
        _entries.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenEverythingIsValid_StoresAScheduledEntry()
    {
        // Arrange
        var calendar = _calendars.Seed(_ownerId);
        var postId = Guid.NewGuid();

        // Act
        var result = await CreateHandler().HandleAsync(Command(calendar.Id, postId, _ownerId));

        // Assert
        var entry = _entries.Entries.Should().ContainSingle().Subject;
        result.CalendarEntryId.Should().Be(entry.Id);
        entry.ContentCalendarId.Should().Be(calendar.Id);
        entry.PostId.Should().Be(postId);
        entry.ScheduledAt.Should().Be(_scheduledAt);
        entry.Status.Should().Be(CalendarEntryStatus.Scheduled);
    }

    [Fact]
    public async Task HandleAsync_AsksPostsForTheCallersPost()
    {
        // Arrange
        var calendar = _calendars.Seed(_ownerId);
        var postId = Guid.NewGuid();

        // Act
        await CreateHandler().HandleAsync(Command(calendar.Id, postId, _ownerId));

        // Assert
        var call = _posts.Calls.Should().ContainSingle().Subject;
        call.UserId.Should().Be(_ownerId);
        call.PostIds.Should().Equal(postId);
    }
}
