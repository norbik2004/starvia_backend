using StarviaBackend.Shared.Abstractions.Auth;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Application.Posts.Access;
using StarviaBackend.Modules.Posts.Application.Posts.Queries.BrowsePosts;
using StarviaBackend.Modules.Posts.Core.Posts.Access;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Configurations.Read.Models;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Queries;

internal sealed class BrowsePostsHandler(PostReadDbContext dbContext, IPostAccessService access)
    : IQueryHandler<BrowsePostsQuery, PagedResult<PostDto>>
{
    public async Task<PagedResult<PostDto>> HandleAsync(BrowsePostsQuery query, CancellationToken cancellationToken = default)
    {
        var scope = access.ScopeFor(PostAction.Browse);
        if (scope.IsEmpty)
        {
            return PagedResult<PostDto>.Empty(query.Page, query.PageSize);
        }

        var posts = ApplyFilters(dbContext.Posts.AsNoTracking(), query, scope);

        var total = await posts.LongCountAsync(cancellationToken);

        // Also guards against Skip() overflowing for absurdly large page numbers.
        var skip = (long)(query.Page - 1) * query.PageSize;
        if (skip >= total)
        {
            return new PagedResult<PostDto>([], query.Page, query.PageSize, total);
        }

        var items = await ApplyOrdering(posts, query)
            .Skip((int)skip)
            .Take(query.PageSize)
            .Select(p => new PostDto(
                p.Id,
                p.Title,
                p.Status,
                p.CreatedAt,
                p.CreatedBy,
                p.LastModifiedAt,
                p.PostAttachments.Count))
            .ToListAsync(cancellationToken);

        return new PagedResult<PostDto>(items, query.Page, query.PageSize, total);
    }

    private static IQueryable<PostReadModel> ApplyFilters(
        IQueryable<PostReadModel> posts,
        BrowsePostsQuery query,
        AccessScope scope)
    {
        if (!scope.Unrestricted)
        {
            var ownerId = scope.OwnerId;
            posts = posts.Where(p => p.CreatedBy == ownerId);
        }

        if (query.CreatedBy is { } createdBy)
        {
            var createdById = createdBy.ToString();
            posts = posts.Where(p => p.CreatedBy == createdById);
        }

        if (query.Status is { } status)
        {
            posts = posts.Where(p => p.Status == status);
        }

        if (query.Search is { } search)
        {
            var pattern = $"%{EscapeLikePattern(search)}%";
            posts = posts.Where(p => Microsoft.EntityFrameworkCore.EF.Functions.ILike(p.Title, pattern, "\\"));
        }

        return posts;
    }

    // Id is the tie-breaker so paging stays stable when the sort column has duplicates.
    private static IQueryable<PostReadModel> ApplyOrdering(IQueryable<PostReadModel> posts, BrowsePostsQuery query) =>
        (query.SortBy, query.Descending) switch
        {
            (PostSortBy.Title, true) => posts.OrderByDescending(p => p.Title).ThenByDescending(p => p.Id),
            (PostSortBy.Title, false) => posts.OrderBy(p => p.Title).ThenBy(p => p.Id),
            (PostSortBy.LastModifiedAt, true) => posts.OrderByDescending(p => p.LastModifiedAt).ThenByDescending(p => p.Id),
            (PostSortBy.LastModifiedAt, false) => posts.OrderBy(p => p.LastModifiedAt).ThenBy(p => p.Id),
            (_, true) => posts.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id),
            (_, false) => posts.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id),
        };

    private static string EscapeLikePattern(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
