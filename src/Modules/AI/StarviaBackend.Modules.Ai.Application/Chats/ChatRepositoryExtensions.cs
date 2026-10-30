using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Modules.Ai.Core.Chats.Repositories;

namespace StarviaBackend.Modules.Ai.Application.Chats;

internal static class ChatRepositoryExtensions
{
    /// <summary>
    /// Loads a chat for modification and makes sure it belongs to the user: chats are private,
    /// so every command goes through here instead of repeating the not-found/ownership checks.
    /// </summary>
    public static async Task<Chat> GetOwnedAsync(
        this IChatRepository repository,
        Guid chatId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var chat = await repository.GetByIdAsync(chatId, cancellationToken)
            ?? throw new ChatNotFoundException(chatId);

        if (!chat.IsOwnedBy(userId))
        {
            throw new ChatNotOwnedException(chatId);
        }

        return chat;
    }
}
