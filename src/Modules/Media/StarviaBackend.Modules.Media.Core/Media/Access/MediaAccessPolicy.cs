using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Media.Core.Media.Access;

/// <summary>
/// Everything a caller can try to do with an existing media file. Uploading is not listed: any
/// authenticated user may upload, and the file then belongs to them. Add a value here, then give
/// it a rule in <see cref="MediaAccessPolicy"/>.
/// </summary>
internal enum MediaAction
{
    Browse,
    View,
    Download,
    Update,
    Delete,
}

/// <summary>
/// THE place where access rules for the Media module are defined. Nothing else in the module
/// compares <c>CreatedBy</c> to a user id or looks at roles: handlers and queries ask
/// <c>IMediaAccessService</c>, which asks this policy.
/// </summary>
internal static class MediaAccessPolicy
{
    public static AccessScope ScopeFor(MediaAction action, AccessActor actor) => action switch
    {
        MediaAction.Browse => actor.OwnerOrAdmin(),
        MediaAction.View => actor.OwnerOrAdmin(),
        MediaAction.Download => actor.OwnerOrAdmin(),
        MediaAction.Update => actor.OwnerOrAdmin(),
        MediaAction.Delete => actor.OwnerOrAdmin(),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "No access rule defined for this action."),
    };
}
