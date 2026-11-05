using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Application.PostAttachments.Queries.BrowsePostAttachments;

/// <summary>
/// Lists the attachments of one post in display order. The caller needs to be allowed to view
/// the post's attachments (see the posts access policy).
/// </summary>
internal sealed record BrowsePostAttachmentsQuery(Guid PostId) : IQuery<IReadOnlyList<PostAttachmentDto>>;

public sealed record PostAttachmentDto(Guid Id, Guid FileId, DateTime CreatedAt, int Order);
