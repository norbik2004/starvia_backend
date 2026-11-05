using StarviaBackend.Modules.Ai.Application.Chats.Access;
using StarviaBackend.Modules.Ai.Core.Chats.Access;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Modules.Ai.Core.Chats.Repositories;

namespace StarviaBackend.Modules.Ai.Application.Chats;

internal static class ChatRepositoryExtensions
{
    /// <summary>
    /// Loads a chat for modification and makes sure the caller may do <paramref name="action"/> on it,
    /// so every command goes through here instead of repeating the not-found/access checks.
    /// </summary>
    /// <exception cref="ChatNotFoundException">The chat does not exist.</exception>
    /// <exception cref="ChatAccessDeniedException">The caller may not do <paramref name="action"/> on it.</exception>
    public static async Task<Chat> GetAllowedAsync(
        this IChatRepository repository,
        IChatAccessService access,
        ChatAction action,
        Guid chatId,
        CancellationToken cancellationToken = default)
    {
        var chat = await repository.GetByIdAsync(chatId, cancellationToken)
            ?? throw new ChatNotFoundException(chatId);

        access.EnsureAllowed(action, chat);

        return chat;
    }
}
