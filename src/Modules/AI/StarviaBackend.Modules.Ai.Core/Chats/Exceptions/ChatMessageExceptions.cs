using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Ai.Core.Chats.Exceptions;

internal sealed class EmptyChatMessageException()
    : BusinessException("Chat message content must not be empty")
{
    public override string Code => "chat_message_empty";
}
