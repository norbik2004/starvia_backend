using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Posts.Core.Posts.Exceptions;


internal sealed class PostNotFoundException(Guid postId)
    : BusinessException($"Post with {postId} was not found")
{
    public override string Code => "post_not_found";
}

internal sealed class PostNotOwnedException(Guid postId)
    : BusinessException($"User is not the owner of post '{postId}'")
{
    public override string Code => "post_not_owned";
}
