using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Queries.BrowsePostAttachments;
using StarviaBackend.Modules.Posts.Application.Posts.Access;
using StarviaBackend.Modules.Posts.Core.Posts.Access;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Queries;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.PostAttachments.Queries;

internal sealed class BrowsePostAttachmentsHandler(PostReadDbContext dbContext, IPostAccessService access)
    : IQueryHandler<BrowsePostAttachmentsQuery, IReadOnlyList<PostAttachmentDto>>
{
    public async Task<IReadOnlyList<PostAttachmentDto>> HandleAsync(
        BrowsePostAttachmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        await dbContext.EnsurePostAccessAsync(access, PostAction.ViewAttachments, query.PostId, cancellationToken);

        return await dbContext.PostAttachments
            .AsNoTracking()
            .Where(pa => pa.PostId == query.PostId)
            .OrderBy(pa => pa.Order)
            .ThenBy(pa => pa.CreatedAt)
            .Select(pa => new PostAttachmentDto(pa.Id, pa.FileId, pa.CreatedAt, pa.Order))
            .ToListAsync(cancellationToken);
    }
}
