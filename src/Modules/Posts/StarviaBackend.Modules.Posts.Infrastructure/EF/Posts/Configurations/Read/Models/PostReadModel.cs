using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Configurations.Read.Models;

internal sealed class PostReadModel
{
    public Guid Id { get; init; }
    public string Title { get; init; }
    public string? Body { get; init; }
    public PostStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
