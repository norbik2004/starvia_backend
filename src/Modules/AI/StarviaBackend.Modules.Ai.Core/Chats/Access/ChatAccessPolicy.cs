using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Ai.Core.Chats.Access;

internal enum ChatAction
{
    Browse,
    View,
    Update,
    Delete,
    AddMessage,
}

internal static class ChatAccessPolicy
{
    public static AccessScope ScopeFor(ChatAction action, AccessActor actor) => action switch
    {
        ChatAction.Browse => actor.OwnerOnly(),
        ChatAction.View => actor.OwnerOnly(),
        ChatAction.Update => actor.OwnerOnly(),
        ChatAction.Delete => actor.OwnerOnly(),
        ChatAction.AddMessage => actor.OwnerOnly(),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "No access rule defined for this action."),
    };
}
