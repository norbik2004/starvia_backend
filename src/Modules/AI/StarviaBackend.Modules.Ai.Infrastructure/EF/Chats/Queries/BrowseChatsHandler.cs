using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Ai.Application.Chats.Queries.BrowseChats;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Queries;

internal sealed class BrowseChatsHandler(AiReadDbContext dbContext)
    : IQueryHandler<BrowseChatsQuery, PagedResult<ChatDto>>
{
    public async Task<PagedResult<ChatDto>> HandleAsync(BrowseChatsQuery query, CancellationToken cancellationToken = default)
    {
        var chats = dbContext.Chats
            .AsNoTracking()
            .Where(c => c.UserId == query.UserId);

        if (query.Type is { } type)
        {
            chats = chats.Where(c => c.Type == type);
        }

        if (query.Status is { } status)
        {
            chats = chats.Where(c => c.Status == status);
        }

        var total = await chats.LongCountAsync(cancellationToken);

        if (total == 0)
        {
            return PagedResult<ChatDto>.Empty(query.Page, query.PageSize);
        }

        // Most recently active first; a chat without messages yet counts from the moment it was created.
        var items = await chats
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ThenBy(c => c.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new ChatDto(c.Id, c.Type, c.Title, c.Status, c.PostId, c.LastMessageAt, c.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ChatDto>(items, query.Page, query.PageSize, total);
    }
}
