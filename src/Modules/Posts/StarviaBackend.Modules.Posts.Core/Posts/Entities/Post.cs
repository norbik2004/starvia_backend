using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Posts.Core.Posts.Entities;

internal sealed class Post : BaseEntity, IAuditable
{
    private readonly List<PostPublication> _postPublications = [];
    private readonly List<PostAttachment> _postAttachments = [];

    public string Title { get; private set; }
    public string? Body { get; private set; }
    public PostStatus Status { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public IReadOnlyCollection<PostPublication> PostPublications => _postPublications.AsReadOnly();
    public IReadOnlyCollection<PostAttachment> PostAttachments => _postAttachments.AsReadOnly();

    private Post()
    {
    }

    public static Post Create(string title)
    {
        return new Post
        {
            Id = Guid.NewGuid(),
            Title = title,
            Status = PostStatus.Draft
        };
    }

    public void UpdateStatus(PostStatus status) => Status = status;

    public void Update(string title, string? body, PostStatus status)
    {
        Title = title;
        Body = body;
        Status = status;
    }
}
