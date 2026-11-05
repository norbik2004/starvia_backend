using StarviaBackend.Modules.Posts.Application.PostAttachments.Queries.BrowsePostAttachments;
using StarviaBackend.Modules.Posts.Application.PostPublications.Queries.BrowsePostPublications;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Application.Posts.Queries.GetPost;

/// <summary>Returns one post if the caller may view it (see the posts access policy).</summary>
internal sealed record GetPostQuery(Guid PostId) : IQuery<PostLongDto>;

public sealed record PostLongDto(Guid Id, string Title, string Body, PostStatus Status,
    DateTime CreatedAt, string CreatedBy, DateTime? LastModifiedAt, ICollection<PostAttachmentDto> postAttachments,
    ICollection<PostPublicationDto> postPublications);
