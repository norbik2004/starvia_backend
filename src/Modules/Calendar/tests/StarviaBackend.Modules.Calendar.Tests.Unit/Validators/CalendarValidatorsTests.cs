using FluentValidation.TestHelper;
using StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.AddCalendarEntry;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.AddContentCalendar;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.RemoveContentCalendar;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.UpdateContentCalendar;

namespace StarviaBackend.Modules.Calendar.Tests.Unit.Validators;

public sealed class AddContentCalendarValidatorTests
{
    private readonly AddContentCalendarValidator _validator = new();

    private static AddContentCalendarCommand Command(string name) => new(new AddContentCalendarRequest(name), Guid.NewGuid());

    [Fact]
    public void Named_calendar_passes()
    {
        _validator.TestValidate(Command("Marketing")).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_name_fails(string name)
    {
        _validator.TestValidate(Command(name))
            .ShouldHaveValidationErrorFor(x => x.request.name)
            .WithErrorMessage("Name is required");
    }
}

public sealed class UpdateContentCalendarValidatorTests
{
    private readonly UpdateContentCalendarValidator _validator = new();

    private static UpdateContentCalendarCommand Command(string name = "Marketing", Guid? calendarId = null) =>
        new(new UpdateContentCalendarRequest(calendarId ?? Guid.NewGuid(), name), Guid.NewGuid());

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(Command()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_name_fails()
    {
        _validator.TestValidate(Command(name: ""))
            .ShouldHaveValidationErrorFor(x => x.request.name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Name_of_100_characters_passes_and_101_fails()
    {
        _validator.TestValidate(Command(name: new string('n', 100))).ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Command(name: new string('n', 101)))
            .ShouldHaveValidationErrorFor(x => x.request.name)
            .WithErrorMessage("Name cannot exceed 100 characters.");
    }

    [Fact]
    public void Empty_calendar_id_fails()
    {
        _validator.TestValidate(Command(calendarId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.request.contentCalendarId)
            .WithErrorMessage("Content Calendar ID is required.");
    }
}

public sealed class RemoveContentCalendarValidatorTests
{
    private readonly RemoveContentCalendarValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var command = new RemoveContentCalendarCommand(new RemoveContentCalendarRequest(Guid.NewGuid()), Guid.NewGuid());

        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_ids_fail()
    {
        var command = new RemoveContentCalendarCommand(new RemoveContentCalendarRequest(Guid.Empty), Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.request.contentCalendarId);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}

public sealed class AddCalendarEntryValidatorTests
{
    private readonly AddCalendarEntryValidator _validator = new();

    private static AddCalendarEntryCommand Command(
        Guid? calendarId = null,
        Guid? postId = null,
        DateTimeOffset? scheduledAt = null) =>
        new(
            new AddCalendarEntryRequest(
                calendarId ?? Guid.NewGuid(),
                postId ?? Guid.NewGuid(),
                scheduledAt ?? DateTimeOffset.UtcNow.AddDays(1)),
            Guid.NewGuid());

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(Command()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_calendar_id_fails()
    {
        _validator.TestValidate(Command(calendarId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.request.CalendarId)
            .WithErrorMessage("CalendarId is required");
    }

    [Fact]
    public void Empty_post_id_fails()
    {
        _validator.TestValidate(Command(postId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.request.PostId)
            .WithErrorMessage("PostId is required");
    }

    [Fact]
    public void Default_scheduled_time_fails()
    {
        _validator.TestValidate(Command(scheduledAt: default(DateTimeOffset)))
            .ShouldHaveValidationErrorFor(x => x.request.ScheduledAt)
            .WithErrorMessage("ScheduledAt is required");
    }
}
