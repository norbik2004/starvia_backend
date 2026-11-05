 using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Ai.Core.Chats.Entities;

internal sealed class ChatMessage : AuditableEntity
{
    public const int MaxModelLength = 100;

    public Guid ChatId { get; private set; }
    public Chat Chat { get; private set; } = null!;

    public int Sequence { get; private set; }

    public ChatMessageRole Role { get; private set; }

    public string Content { get; private set; } = null!;

    public string? Model { get; private set; }

    public int? InputTokens { get; private set; }

    public int? OutputTokens { get; private set; }

    private ChatMessage()
    {
    }

    internal static ChatMessage Create(
        Guid chatId,
        int sequence,
        ChatMessageRole role,
        string content,
        string? model = null,
        int? inputTokens = null,
        int? outputTokens = null)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown chat message role.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new EmptyChatMessageException();
        }

        if (model is { Length: > MaxModelLength })
        {
            throw new ArgumentException($"Model name cannot be longer than {MaxModelLength} characters.", nameof(model));
        }

        if (inputTokens is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(inputTokens), inputTokens, "Token count cannot be negative.");
        }

        if (outputTokens is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(outputTokens), outputTokens, "Token count cannot be negative.");
        }

        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            Sequence = sequence,
            Role = role,
            Content = content,
            Model = model,
            InputTokens = inputTokens,
            OutputTokens = outputTokens
        };
    }
}
