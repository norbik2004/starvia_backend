using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Posts.Core.Posts.Entities;

internal sealed class PostAttachment : AuditableEntity
{
    public Guid PostId { get; private set; }
    public Post Post { get; private set; } = null!;
    public Guid FileId { get; private set; }
    public int Order { get; private set; }

    private PostAttachment()
    {
    }

    public static PostAttachment Create(Guid postId, Guid fileId, int order)
    {
        return new PostAttachment
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            FileId = fileId,
            Order = order
        };
    }
}
