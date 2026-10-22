using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Application.PostAttachments.Queries.BrowsePostAttachments;

internal sealed record BrowsePostAttachmentsQuery : IQuery<PostAttachmentDto>;

public sealed record PostAttachmentDto(Guid Id, Guid FileId, DateTime CreatedAt, int Order);
