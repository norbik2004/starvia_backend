using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Queries.BrowsePostAttachments;
using StarviaBackend.Modules.Posts.Application.Posts.Access;
using StarviaBackend.Modules.Posts.Application.Posts.Queries.GetPost;
using StarviaBackend.Modules.Posts.Application.PostPublications.Queries.BrowsePostPublications;
using StarviaBackend.Modules.Posts.Core.Posts.Access;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Queries;

internal sealed class GetPostHandler(PostReadDbContext dbContext, IPostAccessService access)
    : IQueryHandler<GetPostQuery, PostLongDto>
{
    public async Task<PostLongDto> HandleAsync(GetPostQuery query, CancellationToken cancellationToken = default)
    {
        var post = await dbContext.Posts
            .AsNoTracking()
            .Where(p => p.Id == query.PostId)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Body,
                p.Status,
                p.CreatedAt,
                p.CreatedBy,
                p.LastModifiedAt,
                Attachments = p.PostAttachments
                    .OrderBy(pa => pa.Order)
                    .Select(pa => new { pa.Id, pa.FileId, pa.CreatedAt, pa.Order })
                    .ToList(),
                Publications = p.PostPublications
                    .OrderByDescending(pp => pp.CreatedAt)
                    .ThenByDescending(pp => pp.Id)
                    .Select(pp => new
                    {
                        pp.Id,
                        pp.PostId,
                        pp.Status,
                        pp.UserPlatformId,
                        pp.PublishedAt,
                        pp.ExternalPostId,
                        pp.CreatedAt,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new PostNotFoundException(query.PostId);

        access.EnsureAllowed(PostAction.View, post.Id, post.CreatedBy);

        // Nested resources follow their own rules: if the policy ever lets someone see a post but not
        // its attachments/publications, they are left out instead of failing the whole request.
        var attachmentsVisible = access.ScopeFor(PostAction.ViewAttachments).Allows(post.CreatedBy);
        var publicationsVisible = access.ScopeFor(PostAction.ViewPublications).Allows(post.CreatedBy);

        ICollection<PostAttachmentDto> postAttachments = post.Attachments
            .Where(_ => attachmentsVisible)
            .Select(pa => new PostAttachmentDto(pa.Id, pa.FileId, pa.CreatedAt, pa.Order))
            .ToList()
            .AsReadOnly();

        ICollection<PostPublicationDto> postPublications = post.Publications
            .Where(_ => publicationsVisible)
            .Select(pp => new PostPublicationDto(
                pp.Id,
                pp.PostId,
                pp.Status,
                pp.UserPlatformId,
                pp.PublishedAt,
                pp.ExternalPostId,
                pp.CreatedAt))
            .ToList()
            .AsReadOnly();

        return new PostLongDto(
            post.Id,
            post.Title,
            post.Body ?? "",
            post.Status,
            post.CreatedAt,
            post.CreatedBy,
            post.LastModifiedAt,
            postAttachments,
            postPublications);
    }
}
