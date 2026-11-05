using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Posts.Core.Posts.Access;

/// <summary>
/// Everything a caller can try to do with posts and the resources hanging off them (attachments,
/// publications). Those have no owner of their own: the rule is applied to the owner of the
/// parent post. Add a value here, then give it a rule in <see cref="PostAccessPolicy"/>.
/// </summary>
internal enum PostAction
{
    Browse,
    View,
    Update,
    Delete,
    ViewAttachments,
    ManageAttachments,
    ViewPublications,
}

/// <summary>
/// THE place where access rules for the Posts module are defined. Nothing else in the module
/// compares <c>CreatedBy</c> to a user id or looks at roles: handlers and queries ask
/// <c>IPostAccessService</c>, which asks this policy.
/// </summary>
internal static class PostAccessPolicy
{
    public static AccessScope ScopeFor(PostAction action, AccessActor actor) => action switch
    {
        PostAction.Browse => actor.OwnerOrAdmin(),
        PostAction.View => actor.OwnerOrAdmin(),
        PostAction.Update => actor.OwnerOrAdmin(),
        PostAction.Delete => actor.OwnerOrAdmin(),
        PostAction.ViewAttachments => actor.OwnerOrAdmin(),
        PostAction.ManageAttachments => actor.OwnerOrAdmin(),
        PostAction.ViewPublications => actor.OwnerOrAdmin(),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "No access rule defined for this action."),
    };

    // Need a stricter rule, e.g. "only admins may delete"? Use actor.AdminOnly() in the switch above.
}
