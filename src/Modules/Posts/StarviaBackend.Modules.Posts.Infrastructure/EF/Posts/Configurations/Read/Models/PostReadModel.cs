using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Modules.Posts.Infrastructure.EF.PostAttachments.Configurations.Read.Models;
using StarviaBackend.Modules.Posts.Infrastructure.EF.PostPublications.Configurations.Read.Models;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Configurations.Read.Models;

internal sealed class PostReadModel
{
    public Guid Id { get; init; }
    public string Title { get; init; }
    public string? Body { get; init; }
    public PostStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public ICollection<PostAttachmentReadModel> PostAttachments { get; init; } = new List<PostAttachmentReadModel>();
}
