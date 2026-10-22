using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.PostPublications.Configurations.Read.Models;

internal sealed class PostPublicationReadModel
{
    public Guid Id { get; init; }
    public Guid PostId { get; init; }
    public PostPublicationStatus Status { get; init; }
    public DateTime? PublishedAt { get; init; }
    public string? ExternalPostId { get; init; }
    public Guid UserPlatformId { get; init; }
}
