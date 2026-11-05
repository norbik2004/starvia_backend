using StarviaBackend.Modules.Ai.Core.Chats.Access;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Ai.Core.Chats.Exceptions;

internal sealed class ChatNotFoundException(Guid chatId)
    : BusinessException($"Chat with {chatId} was not found")
{
    public override string Code => "chat_not_found";
}

internal sealed class ChatAccessDeniedException(Guid chatId, ChatAction action)
    : ForbiddenException($"You are not allowed to {action} on chat '{chatId}'")
{
    public override string Code => "chat_access_denied";
}

internal sealed class ChatArchivedException(Guid chatId)
    : BusinessException($"Chat '{chatId}' is archived and cannot receive new messages")
{
    public override string Code => "chat_archived";
}

internal sealed class InvalidChatTitleException(int maxLength)
    : BusinessException($"Chat title must not be empty and cannot be longer than {maxLength} characters")
{
    public override string Code => "chat_title_invalid";
}

internal sealed class ChatTypeNotSupportedException(Guid chatId, ChatType chatType, ChatType expected)
    : BusinessException($"Chat '{chatId}' is a '{chatType}' chat but this operation needs a '{expected}' chat")
{
    public override string Code => "chat_type_not_supported";
}

internal sealed class PostLinkNotSupportedException(Guid chatId, ChatType chatType)
    : BusinessException($"Chat '{chatId}' of type '{chatType}' cannot be linked to a post")
{
    public override string Code => "chat_post_link_not_supported";
}
