using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Ai.Core.Chats.Entities;

/// <summary>
/// A conversation between a user and one of the AI agents (see <see cref="ChatType"/>).
/// It is the aggregate root of the chat history: every prompt and every AI answer is a
/// <see cref="ChatMessage"/> owned by the chat.
/// </summary>
internal sealed class Chat : AuditableEntity
{
    public const int MaxTitleLength = 150;

    private readonly List<ChatMessage> _messages = [];

    /// <summary>Owner of the chat. Only the owner can see and continue the conversation.</summary>
    public Guid UserId { get; private set; }

    public ChatType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public ChatStatus Status { get; private set; }

    /// <summary>
    /// Post (from the Posts module, referenced by id only) that this chat produced or is refining.
    /// Supported only by <see cref="ChatType.PostContentGeneration"/> chats.
    /// </summary>
    public Guid? PostId { get; private set; }

    /// <summary>When the last message was added. Used to order the chat history.</summary>
    public DateTime? LastMessageAt { get; private set; }

    /// <summary>The messages of the chat, ordered by <see cref="ChatMessage.Sequence"/>.</summary>
    public IReadOnlyCollection<ChatMessage> Messages => _messages.AsReadOnly();

    private Chat()
    {
    }

    public static Chat Create(Guid userId, ChatType type, string title)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id must not be empty.", nameof(userId));
        }

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown chat type.");
        }

        return new Chat
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = NormalizeTitle(title),
            Status = ChatStatus.Active
        };
    }

    public bool IsOwnedBy(Guid userId) => UserId == userId;

    public void Rename(string title)
    {
        Title = NormalizeTitle(title);
    }

    public void Archive()
    {
        Status = ChatStatus.Archived;
    }

    public void Unarchive()
    {
        Status = ChatStatus.Active;
    }

    /// <summary>Links the chat to the post it produced. Allowed only for post content generation chats.</summary>
    public void LinkPost(Guid postId)
    {
        if (Type != ChatType.PostContentGeneration)
        {
            throw new PostLinkNotSupportedException(Id, Type);
        }

        if (postId == Guid.Empty)
        {
            throw new ArgumentException("Post id must not be empty.", nameof(postId));
        }

        PostId = postId;
    }

    /// <summary>Stores a prompt written by the user.</summary>
    /// <param name="content">The prompt, kept exactly as typed.</param>
    /// <param name="utcNow">Current UTC time, used to update <see cref="LastMessageAt"/>.</param>
    public ChatMessage AddUserMessage(string content, DateTime utcNow)
        => AddMessage(ChatMessageRole.User, content, utcNow);

    /// <summary>Stores an answer produced by the AI together with the model and token usage it reported.</summary>
    public ChatMessage AddAssistantMessage(
        string content,
        DateTime utcNow,
        string? model = null,
        int? inputTokens = null,
        int? outputTokens = null)
        => AddMessage(ChatMessageRole.Assistant, content, utcNow, model, inputTokens, outputTokens);

    /// <summary>Stores the instructions given to the model, so the exact context of the chat can be replayed.</summary>
    public ChatMessage AddSystemMessage(string content, DateTime utcNow)
        => AddMessage(ChatMessageRole.System, content, utcNow);

    private ChatMessage AddMessage(
        ChatMessageRole role,
        string content,
        DateTime utcNow,
        string? model = null,
        int? inputTokens = null,
        int? outputTokens = null)
    {
        if (Status == ChatStatus.Archived)
        {
            throw new ChatArchivedException(Id);
        }

        var sequence = _messages.Count == 0 ? 1 : _messages.Max(m => m.Sequence) + 1;
        var message = ChatMessage.Create(Id, sequence, role, content, model, inputTokens, outputTokens);

        _messages.Add(message);
        LastMessageAt = utcNow;

        return message;
    }

    private static string NormalizeTitle(string title)
    {
        var normalized = title?.Trim();

        if (string.IsNullOrEmpty(normalized) || normalized.Length > MaxTitleLength)
        {
            throw new InvalidChatTitleException(MaxTitleLength);
        }

        return normalized;
    }
}
