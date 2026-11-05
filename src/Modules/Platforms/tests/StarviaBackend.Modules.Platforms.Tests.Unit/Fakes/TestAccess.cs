using StarviaBackend.Modules.Platforms.Application.Access;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Modules.Platforms.Tests.Unit.Fakes;

/// <summary>Builds the real <see cref="PlatformAccessService"/> for a fake caller, so handler tests exercise the real rules.</summary>
internal static class TestAccess
{
    public static IPlatformAccessService For(Guid? userId, bool isAdmin = false) =>
        new PlatformAccessService(new FakeContext(new FakeIdentity(userId, isAdmin)));

    public static IPlatformAccessService Admin() => For(Guid.NewGuid(), isAdmin: true);

    public static IPlatformAccessService Anonymous() => For(null);

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


