using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Ai.Application.Chats.Access;
using StarviaBackend.Modules.Ai.Application.Chats.Queries.BrowseChats;
using StarviaBackend.Modules.Ai.Core.Chats.Access;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Configurations.Read.Models;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Queries;

internal sealed class BrowseChatsHandler(AiReadDbContext dbContext, IChatAccessService access)
    : IQueryHandler<BrowseChatsQuery, PagedResult<ChatDto>>
{
    public async Task<PagedResult<ChatDto>> HandleAsync(BrowseChatsQuery query, CancellationToken cancellationToken = default)
    {
        var scope = access.ScopeFor(ChatAction.Browse);
        if (scope.IsEmpty)
        {
            return PagedResult<ChatDto>.Empty(query.Page, query.PageSize);
        }

        var chats = ApplyFilters(dbContext.Chats.AsNoTracking(), query, scope);

        var total = await chats.LongCountAsync(cancellationToken);

        // Also guards against Skip() overflowing for absurdly large page numbers.
        var skip = (long)(query.Page - 1) * query.PageSize;
        if (skip >= total)
        {
            return new PagedResult<ChatDto>([], query.Page, query.PageSize, total);
        }

        var items = await ApplyOrdering(chats, query)
            .Skip((int)skip)
            .Take(query.PageSize)
            .Select(c => new ChatDto(c.Id, c.Type, c.Title, c.Status, c.PostId, c.LastMessageAt, c.CreatedAt, c.UserId))
            .ToListAsync(cancellationToken);

        return new PagedResult<ChatDto>(items, query.Page, query.PageSize, total);
    }

    private static IQueryable<ChatReadModel> ApplyFilters(
        IQueryable<ChatReadModel> chats,
        BrowseChatsQuery query,
        AccessScope scope)
    {
        if (!scope.Unrestricted && Guid.TryParse(scope.OwnerId, out var scopeOwnerId))
        {
            chats = chats.Where(c => c.UserId == scopeOwnerId);
        }

        if (query.OwnerId is { } ownerId)
        {
            chats = chats.Where(c => c.UserId == ownerId);
        }

        if (query.Type is { } type)
        {
            chats = chats.Where(c => c.Type == type);
        }

        if (query.Status is { } status)
        {
            chats = chats.Where(c => c.Status == status);
        }

        if (query.PostId is { } postId)
        {
            chats = chats.Where(c => c.PostId == postId);
        }

        if (query.Search is { } search)
        {
            var pattern = $"%{EscapeLikePattern(search)}%";
            chats = chats.Where(c => Microsoft.EntityFrameworkCore.EF.Functions.ILike(c.Title, pattern, "\\"));
        }

        return chats;
    }

    // Id is the tie-breaker so paging stays stable when the sort column has duplicates. A chat without
    // messages yet counts as active from the moment it was created.
    private static IQueryable<ChatReadModel> ApplyOrdering(IQueryable<ChatReadModel> chats, BrowseChatsQuery query) =>
        (query.SortBy, query.Descending) switch
        {
            (ChatSortBy.Title, true) => chats.OrderByDescending(c => c.Title).ThenByDescending(c => c.Id),
            (ChatSortBy.Title, false) => chats.OrderBy(c => c.Title).ThenBy(c => c.Id),
            (ChatSortBy.CreatedAt, true) => chats.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id),
            (ChatSortBy.CreatedAt, false) => chats.OrderBy(c => c.CreatedAt).ThenBy(c => c.Id),
            (_, true) => chats.OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt).ThenByDescending(c => c.Id),
            (_, false) => chats.OrderBy(c => c.LastMessageAt ?? c.CreatedAt).ThenBy(c => c.Id),
        };

    private static string EscapeLikePattern(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
