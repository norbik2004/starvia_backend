using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.AddChatMessage;

internal sealed record AddChatMessageCommand(Guid ChatId, AddChatMessageRequest Request, Guid UserId)
    : ICommand<AddChatMessageResult>;

/// <summary>A prompt written by the user. Answers of the AI are stored by the AI flow, never sent by clients.</summary>
internal sealed record AddChatMessageRequest(string Content)
{
    public const int MaxContentLength = 8000;
}

internal sealed record AddChatMessageResult(Guid MessageId, int Sequence);
