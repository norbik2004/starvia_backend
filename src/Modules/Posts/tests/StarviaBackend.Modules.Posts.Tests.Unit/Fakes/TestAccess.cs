using StarviaBackend.Modules.Posts.Application.Posts.Access;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Fakes;

/// <summary>Builds the real <see cref="PostAccessService"/> for a fake caller, so handler tests exercise the real rules.</summary>
internal static class TestAccess
{
    public static IPostAccessService For(Guid? userId, bool isAdmin = false) =>
        new PostAccessService(new FakeContext(new FakeIdentity(userId, isAdmin)));

    public static IPostAccessService Admin() => For(Guid.NewGuid(), isAdmin: true);

    public static IPostAccessService Anonymous() => For(null);

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
