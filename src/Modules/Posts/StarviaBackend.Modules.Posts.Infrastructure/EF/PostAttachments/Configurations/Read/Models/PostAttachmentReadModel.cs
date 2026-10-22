using System;
using System.Collections.Generic;
using System.Text;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.PostAttachments.Configurations.Read.Models;

internal class PostAttachmentReadModel
{
    public Guid Id { get; init; }
    public Guid PostId { get; init; }
    public Guid FileId { get; init; }
    public int Order { get; init; }
    public DateTime CreatedAt { get; init; }
}
