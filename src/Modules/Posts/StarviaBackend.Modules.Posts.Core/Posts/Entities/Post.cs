using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Posts.Core.Posts.Entities;

internal sealed class Post : BaseEntity, IAuditable
{
    public string Title { get; private set; }
    public string? Body { get; set; }
    public PostStatus Status { get; private set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private Post()
    {
    }

    public static Post Create(string title, DateTime createdAt, Guid createdBy, string? body)
    {
        return new Post
        {
            Id = Guid.NewGuid(),
            Title = title,
            CreatedAt = createdAt,
            CreatedBy = createdBy.ToString(),
            Body = body
        };
    }

    public void UpdateStatus(PostStatus status) => Status = status;
}
