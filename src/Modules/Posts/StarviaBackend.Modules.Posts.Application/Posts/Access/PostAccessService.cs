using StarviaBackend.Modules.Posts.Core.Posts.Access;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Modules.Posts.Application.Posts.Access;

/// <summary>
/// How handlers and queries consult <see cref="PostAccessPolicy"/> for the caller of the
/// current request. Holds no rules itself.
/// </summary>
internal interface IPostAccessService
{
    /// <summary>The posts the current caller may use for <paramref name="action"/>; apply it as a filter when listing.</summary>
    AccessScope ScopeFor(PostAction action);

    /// <summary>Throws <see cref="PostAccessDeniedException"/> unless the caller may do <paramref name="action"/> on the post.</summary>
    void EnsureAllowed(PostAction action, Guid postId, string? postOwnerId);

    void EnsureAllowed(PostAction action, Post post) => EnsureAllowed(action, post.Id, post.CreatedBy);
}

internal sealed class PostAccessService(IContext context) : IPostAccessService
{
    public AccessScope ScopeFor(PostAction action) => PostAccessPolicy.ScopeFor(action, CurrentActor());

    public void EnsureAllowed(PostAction action, Guid postId, string? postOwnerId)
    {
        if (!ScopeFor(action).Allows(postOwnerId))
        {
            throw new PostAccessDeniedException(postId, action);
        }
    }

    private AccessActor CurrentActor() => AccessActor.From(context.Identity);
}
