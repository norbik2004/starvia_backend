using StarviaBackend.Modules.Ai.Core.Chats.Access;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Modules.Ai.Application.Chats.Access;

/// <summary>
/// How handlers and queries consult <see cref="ChatAccessPolicy"/> for the caller of the current
/// request. Holds no rules itself.
/// </summary>
internal interface IChatAccessService
{
    /// <summary>The chats the current caller may use for <paramref name="action"/>; apply it as a filter when listing.</summary>
    AccessScope ScopeFor(ChatAction action);

    /// <summary>Throws <see cref="ChatAccessDeniedException"/> unless the caller may do <paramref name="action"/> on the chat.</summary>
    void EnsureAllowed(ChatAction action, Guid chatId, Guid chatOwnerId);

    void EnsureAllowed(ChatAction action, Chat chat) => EnsureAllowed(action, chat.Id, chat.UserId);
}

internal sealed class ChatAccessService(IContext context) : IChatAccessService
{
    public AccessScope ScopeFor(ChatAction action) =>
        ChatAccessPolicy.ScopeFor(action, AccessActor.From(context.Identity));

    public void EnsureAllowed(ChatAction action, Guid chatId, Guid chatOwnerId)
    {
        if (!ScopeFor(action).Allows(chatOwnerId.ToString()))
        {
            throw new ChatAccessDeniedException(chatId, action);
        }
    }
}
