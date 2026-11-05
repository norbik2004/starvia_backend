using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Ai.Application.Chats.Queries.GetChat;

/// <summary>One chat with its history, if the caller may view it (see the chat access policy).</summary>
internal sealed record GetChatQuery(Guid ChatId) : IQuery<ChatLongDto>;

/// <summary>A chat together with its whole history, ordered from the first message to the last.</summary>
public sealed record ChatLongDto(
    Guid Id,
    ChatType Type,
    string Title,
    ChatStatus Status,
    Guid? PostId,
    DateTime? LastMessageAt,
    DateTime CreatedAt,
    Guid OwnerId,
    ICollection<ChatMessageDto> Messages);

public sealed record ChatMessageDto(
    Guid Id,
    int Sequence,
    ChatMessageRole Role,
    string Content,
    string? Model,
    int? InputTokens,
    int? OutputTokens,
    DateTime CreatedAt);
