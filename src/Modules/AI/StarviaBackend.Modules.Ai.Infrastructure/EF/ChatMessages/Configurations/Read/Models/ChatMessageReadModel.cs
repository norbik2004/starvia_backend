using StarviaBackend.Modules.Ai.Core.Chats.Enums;

namespace StarviaBackend.Modules.Ai.Infrastructure.EF.ChatMessages.Configurations.Read.Models;

internal sealed class ChatMessageReadModel
{
    public Guid Id { get; init; }
    public Guid ChatId { get; init; }
    public int Sequence { get; init; }
    public ChatMessageRole Role { get; init; }
    public string Content { get; init; } = string.Empty;
    public string? Model { get; init; }
    public int? InputTokens { get; init; }
    public int? OutputTokens { get; init; }
    public DateTime CreatedAt { get; init; }
}
