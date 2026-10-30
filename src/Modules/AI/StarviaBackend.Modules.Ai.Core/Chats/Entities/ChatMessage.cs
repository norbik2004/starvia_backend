using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Ai.Core.Chats.Entities;

/// <summary>
/// A single entry of a <see cref="Chat"/>: a prompt written by the user or an answer produced by the AI.
/// Messages are created only through the owning <see cref="Chat"/>, which keeps the history consistent.
/// </summary>
internal sealed class ChatMessage : AuditableEntity
{
    public const int MaxModelLength = 100;

    public Guid ChatId { get; private set; }
    public Chat Chat { get; private set; } = null!;

    /// <summary>1-based position of the message inside the chat. Defines the order of the history.</summary>
    public int Sequence { get; private set; }

    public ChatMessageRole Role { get; private set; }

    /// <summary>The raw text of the prompt or answer, stored exactly as received.</summary>
    public string Content { get; private set; } = null!;

    /// <summary>Name of the AI model that produced the message. Set only for assistant messages.</summary>
    public string? Model { get; private set; }

    /// <summary>Tokens consumed by the request that produced this message, when reported by the provider.</summary>
    public int? InputTokens { get; private set; }

    /// <summary>Tokens generated for this message, when reported by the provider.</summary>
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
