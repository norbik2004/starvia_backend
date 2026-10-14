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

internal sealed class TooManyPostAttachmentsException(int maxAttachments)
    : BusinessException($"A post cannot have more than {maxAttachments} attachments")
{
    public override string Code => "too_many_post_attachments";
}

internal sealed class InvalidPostAttachmentOrderException(int order, int attachmentsCount)
    : BusinessException($"Order {order} is out of range. Allowed values are 0 to {attachmentsCount - 1}")
{
    public override string Code => "invalid_post_attachment_order";
}
