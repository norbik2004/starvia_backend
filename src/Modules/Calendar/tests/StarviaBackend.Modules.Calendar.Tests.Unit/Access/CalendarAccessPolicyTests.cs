using FluentAssertions;
using StarviaBackend.Modules.Calendar.Core.Calendars.Access;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Tests.Unit.Fakes;
using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Calendar.Tests.Unit.Access;

public sealed class CalendarAccessPolicyTests
{
    private static CalendarAction[] AllActions => Enum.GetValues<CalendarAction>();

    [Fact]
    public void ScopeFor_AnAdmin_CoversEveryCalendarForEveryAction()
    {
        foreach (var action in AllActions)
        {
            var scope = CalendarAccessPolicy.ScopeFor(action, new AccessActor(Guid.NewGuid(), IsAdmin: true));

            scope.Allows(Guid.NewGuid().ToString()).Should().BeTrue($"admins may {action}");
        }
    }

    [Fact]
    public void ScopeFor_APlainUser_CoversOnlyTheirOwnCalendarsForEveryAction()
    {
        var userId = Guid.NewGuid();

        foreach (var action in AllActions)
        {
            var scope = CalendarAccessPolicy.ScopeFor(action, new AccessActor(userId, IsAdmin: false));

            scope.Allows(userId.ToString()).Should().BeTrue($"owners may {action}");
            scope.Allows(Guid.NewGuid().ToString()).Should().BeFalse($"strangers may not {action}");
            scope.Allows(null).Should().BeFalse();
        }
    }

    [Fact]
    public void ScopeFor_AnAnonymousCaller_CoversNothingForEveryAction()
    {
        foreach (var action in AllActions)
        {
            var scope = CalendarAccessPolicy.ScopeFor(action, AccessActor.Anonymous);

            scope.IsEmpty.Should().BeTrue($"anonymous callers may not {action}");
        }
    }
}

public sealed class CalendarAccessServiceTests
{
    [Fact]
    public void EnsureAllowed_ForTheOwnerAndForAnAdmin_DoesNotThrow()
    {
        var owner = Guid.NewGuid();

        var asOwner = () => TestAccess.For(owner).EnsureAllowed(CalendarAction.ViewCalendar, Guid.NewGuid(), owner.ToString());
        var asAdmin = () => TestAccess.Admin().EnsureAllowed(CalendarAction.DeleteCalendar, Guid.NewGuid(), owner.ToString());

        asOwner.Should().NotThrow();
        asAdmin.Should().NotThrow();
    }

    [Fact]
    public void EnsureAllowed_ForAnotherUserOrAnonymous_ThrowsAccessDenied()
    {
        var calendarId = Guid.NewGuid();
        var ownerId = Guid.NewGuid().ToString();

        var asStranger = () => TestAccess.For(Guid.NewGuid()).EnsureAllowed(CalendarAction.UpdateCalendar, calendarId, ownerId);
        var asAnonymous = () => TestAccess.Anonymous().EnsureAllowed(CalendarAction.ViewEntries, calendarId, ownerId);

        asStranger.Should().Throw<CalendarAccessDeniedException>().Which.Code.Should().Be("calendar_access_denied");
        asAnonymous.Should().Throw<CalendarAccessDeniedException>();
    }
}
