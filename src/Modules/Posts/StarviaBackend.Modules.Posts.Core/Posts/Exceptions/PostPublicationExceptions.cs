using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Posts.Core.Posts.Exceptions;

internal sealed class PostPublicationNotFoundException(Guid postPublicationId)
    : BusinessException($"Post publication with {postPublicationId} was not found")
{
    public override string Code => "post_publication_not_found";
}
