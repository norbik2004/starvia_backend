using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Application.Posts.Access;
using StarviaBackend.Modules.Posts.Application.PostPublications.Queries.BrowsePostPublications;
using StarviaBackend.Modules.Posts.Core.Posts.Access;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Queries;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.PostPublications.Queries;

internal sealed class BrowsePostPublicationsHandler(PostReadDbContext dbContext, IPostAccessService access)
    : IQueryHandler<BrowsePostPublicationsQuery, IReadOnlyList<PostPublicationDto>>
{
    public async Task<IReadOnlyList<PostPublicationDto>> HandleAsync(
        BrowsePostPublicationsQuery query,
        CancellationToken cancellationToken = default)
    {
        await dbContext.EnsurePostAccessAsync(access, PostAction.ViewPublications, query.PostId, cancellationToken);

        var publications = dbContext.PostPublications
            .AsNoTracking()
            .Where(pp => pp.PostId == query.PostId);

        if (query.Status is { } status)
        {
            publications = publications.Where(pp => pp.Status == status);
        }

        if (query.UserPlatformId is { } userPlatformId)
        {
            publications = publications.Where(pp => pp.UserPlatformId == userPlatformId);
        }

        return await publications
            .OrderByDescending(pp => pp.CreatedAt)
            .ThenByDescending(pp => pp.Id)
            .Select(pp => new PostPublicationDto(
                pp.Id,
                pp.PostId,
                pp.Status,
                pp.UserPlatformId,
                pp.PublishedAt,
                pp.ExternalPostId,
                pp.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
