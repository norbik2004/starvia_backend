using FluentAssertions;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;
using StarviaBackend.Modules.Calendar.Core.Calendars.Enums;

namespace StarviaBackend.Modules.Calendar.Tests.Unit.Entities;

public sealed class ContentCalendarTests
{
    [Fact]
    public void Create_AssignsIdAndName()
    {
        // Act
        var calendar = ContentCalendar.Create("Marketing");

        // Assert
        calendar.Id.Should().NotBeEmpty();
        calendar.Name.Should().Be("Marketing");
        calendar.CalendarEntries.Should().BeEmpty();
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        ContentCalendar.Create("One").Id.Should().NotBe(ContentCalendar.Create("Two").Id);
    }

    [Fact]
    public void UpdateName_ReplacesTheName()
    {
        // Arrange
        var calendar = ContentCalendar.Create("Old");

        // Act
        calendar.UpdateName("New");

        // Assert
        calendar.Name.Should().Be("New");
    }
}

public sealed class CalendarEntryTests
{
    [Fact]
    public void Create_StartsAsScheduledForTheGivenPostAndTime()
    {
        // Arrange
        var calendarId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var scheduledAt = new DateTimeOffset(2026, 12, 24, 10, 30, 0, TimeSpan.FromHours(1));

        // Act
        var entry = CalendarEntry.Create(calendarId, postId, scheduledAt);

        // Assert
        entry.Id.Should().NotBeEmpty();
        entry.ContentCalendarId.Should().Be(calendarId);
        entry.PostId.Should().Be(postId);
        entry.ScheduledAt.Should().Be(scheduledAt);
        entry.Status.Should().Be(CalendarEntryStatus.Scheduled);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        // Act
        var first = CalendarEntry.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        var second = CalendarEntry.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        // Assert
        first.Id.Should().NotBe(second.Id);
    }
}
