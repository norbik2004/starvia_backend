using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Queries;
using StarviaBackend.Modules.Posts.Application.Posts.Queries.BrowsePosts;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;
using Microsoft.EntityFrameworkCore;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Queries;

internal sealed class BrowsePostsHandler(PostReadDbContext dbContext) : IQueryHandler<BrowsePostsQuery, PagedResult<PostDto>>
{
    public async Task<PagedResult<PostDto>> HandleAsync(BrowsePostsQuery query, CancellationToken cancellationToken = default)
    {
        var total = await dbContext.Posts.LongCountAsync(cancellationToken);

        if(total == 0)
        {
            return PagedResult<PostDto>.Empty(query.Page, query.PageSize);
        }

        var items = await dbContext.Posts
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new PostDto
            (
                p.Id,
                p.Title,
                p.CreatedAt,
                p.CreatedBy
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<PostDto>(items, query.Page, query.PageSize, total);
    }
}
