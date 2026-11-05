using StarviaBackend.Modules.Platforms.Core.Platforms.Access;
using StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Modules.Platforms.Application.Access;

/// <summary>
/// How handlers and queries consult <see cref="PlatformAccessPolicy"/> for the caller of the
/// current request. Holds no rules itself.
/// </summary>
internal interface IPlatformAccessService
{
    /// <summary>The resources the current caller may use for <paramref name="action"/>; apply it as a filter when listing.</summary>
    AccessScope ScopeFor(PlatformAction action);

    /// <summary>Throws <see cref="UserPlatformAccessDeniedException"/> unless the caller may do <paramref name="action"/> on the user platform.</summary>
    void EnsureAllowed(PlatformAction action, Guid userPlatformId, string? ownerId);
}

internal sealed class PlatformAccessService(IContext context) : IPlatformAccessService
{
    public AccessScope ScopeFor(PlatformAction action) =>
        PlatformAccessPolicy.ScopeFor(action, AccessActor.From(context.Identity));

    public void EnsureAllowed(PlatformAction action, Guid userPlatformId, string? ownerId)
    {
        if (!ScopeFor(action).Allows(ownerId))
        {
            throw new UserPlatformAccessDeniedException(userPlatformId, action);
        }
    }
}
