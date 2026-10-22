using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Posts.Core.Posts.Entities;

internal sealed class PostPublication : BaseEntity, IAuditable
{
    public Guid PostId { get; private set; }
    public Post Post { get; private set; } = null!;
    public PostPublicationStatus Status { get; private set; }
    public DateTime? PublishedAt { get; set; }
    public string? ExternalPostId { get; set; }
    public Guid UserPlatformId { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private PostPublication()
    {

    }

    public static PostPublication Create(Guid postId, PostPublicationStatus status, Guid userPlatformId,
        DateTime? publishedAt, string? externalPostId)
    {
        return new PostPublication
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            Status = status,
            UserPlatformId = userPlatformId,
            PublishedAt = publishedAt,
            ExternalPostId = externalPostId
        };
    }
}
