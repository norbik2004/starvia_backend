using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Ai.Application.Chats.Queries.GetChat;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Queries;

internal sealed class GetChatHandler(AiReadDbContext dbContext) : IQueryHandler<GetChatQuery, ChatLongDto>
{
    public async Task<ChatLongDto> HandleAsync(GetChatQuery query, CancellationToken cancellationToken = default)
    {
        var chat = await dbContext.Chats
            .AsNoTracking()
            .Include(c => c.Messages.OrderBy(m => m.Sequence))
            .FirstOrDefaultAsync(c => c.Id == query.ChatId, cancellationToken)
            ?? throw new ChatNotFoundException(query.ChatId);

        if (chat.UserId != query.UserId)
        {
            throw new ChatNotOwnedException(query.ChatId);
        }

        ICollection<ChatMessageDto> messages = chat.Messages
            .OrderBy(m => m.Sequence)
            .Select(m => new ChatMessageDto(
                m.Id, m.Sequence, m.Role, m.Content, m.Model, m.InputTokens, m.OutputTokens, m.CreatedAt))
            .ToList()
            .AsReadOnly();

        return new ChatLongDto(
            chat.Id, chat.Type, chat.Title, chat.Status, chat.PostId, chat.LastMessageAt, chat.CreatedAt, messages);
    }
}
