using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Application.Posts.Access;
using StarviaBackend.Modules.Posts.Application.PostPublications.Queries.BrowsePostPublications;
using StarviaBackend.Modules.Posts.Application.PostPublications.Queries.GetPostPublication;
using StarviaBackend.Modules.Posts.Core.Posts.Access;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.PostPublications.Queries;

internal sealed class GetPostPublicationHandler(PostReadDbContext dbContext, IPostAccessService access)
    : IQueryHandler<GetPostPublicationQuery, PostPublicationDto>
{
    public async Task<PostPublicationDto> HandleAsync(
        GetPostPublicationQuery query,
        CancellationToken cancellationToken = default)
    {
        // Publications have no owner of their own: the rule is checked against the owner of the parent post.
        var item = await (
                from pp in dbContext.PostPublications.AsNoTracking()
                join p in dbContext.Posts.AsNoTracking() on pp.PostId equals p.Id
                where pp.Id == query.PostPublicationId
                select new { Publication = pp, PostOwnerId = p.CreatedBy })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new PostPublicationNotFoundException(query.PostPublicationId);

        access.EnsureAllowed(PostAction.ViewPublications, item.Publication.PostId, item.PostOwnerId);

        var publication = item.Publication;
        return new PostPublicationDto(
            publication.Id,
            publication.PostId,
            publication.Status,
            publication.UserPlatformId,
            publication.PublishedAt,
            publication.ExternalPostId,
            publication.CreatedAt);
    }
}
