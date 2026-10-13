using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Posts.Core.Posts.Exceptions;


internal sealed class PostAttachmentNotFoundException(Guid postAttachmentId)
    : BusinessException($"Post attachment with {postAttachmentId} was not found")
{
    public override string Code => "post_attachment_not_found";
}
