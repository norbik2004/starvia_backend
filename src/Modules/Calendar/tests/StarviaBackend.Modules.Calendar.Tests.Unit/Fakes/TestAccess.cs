using StarviaBackend.Modules.Calendar.Application.Access;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Modules.Calendar.Tests.Unit.Fakes;

/// <summary>Builds the real <see cref="CalendarAccessService"/> for a fake caller, so handler tests exercise the real rules.</summary>
internal static class TestAccess
{
    public static ICalendarAccessService For(Guid? userId, bool isAdmin = false) =>
        new CalendarAccessService(new FakeContext(new FakeIdentity(userId, isAdmin)));

    public static ICalendarAccessService Admin() => For(Guid.NewGuid(), isAdmin: true);

    public static ICalendarAccessService Anonymous() => For(null);

    private sealed class FakeContext(IIdentityContext identity) : IContext
    {
        public string RequestId => "test-request";
        public string CorrelationId => "test-correlation";
        public string? TraceId => null;
        public IIdentityContext Identity { get; } = identity;
    }

    private sealed class FakeIdentity(Guid? userId, bool isAdmin) : IIdentityContext
    {
        public bool IsAuthenticated => userId is not null;
        public Guid? UserId => userId;
        public string? Email => null;
        public IReadOnlyList<string> Roles { get; } = isAdmin ? [UserRoles.Admin] : userId is null ? [] : [UserRoles.User];
    }
}
