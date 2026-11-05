using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Ai.Application.Chats.Access;
using StarviaBackend.Modules.Ai.Application.Chats.Queries.GetChat;
using StarviaBackend.Modules.Ai.Core.Chats.Access;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Queries;

internal sealed class GetChatHandler(AiReadDbContext dbContext, IChatAccessService access)
    : IQueryHandler<GetChatQuery, ChatLongDto>
{
    public async Task<ChatLongDto> HandleAsync(GetChatQuery query, CancellationToken cancellationToken = default)
    {
        // The history is only loaded once the caller is known to be allowed to read it.
        var chat = await dbContext.Chats
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.ChatId, cancellationToken)
            ?? throw new ChatNotFoundException(query.ChatId);

        access.EnsureAllowed(ChatAction.View, chat.Id, chat.UserId);

        ICollection<ChatMessageDto> messages = await dbContext.ChatMessages
            .AsNoTracking()
            .Where(m => m.ChatId == chat.Id)
            .OrderBy(m => m.Sequence)
            .Select(m => new ChatMessageDto(
                m.Id, m.Sequence, m.Role, m.Content, m.Model, m.InputTokens, m.OutputTokens, m.CreatedAt))
            .ToListAsync(cancellationToken);

        return new ChatLongDto(
            chat.Id, chat.Type, chat.Title, chat.Status, chat.PostId, chat.LastMessageAt, chat.CreatedAt, chat.UserId, messages);
    }
}
