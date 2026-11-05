using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Ai.Core.Chats.Entities;

internal sealed class Chat : AuditableEntity
{
    public const int MaxTitleLength = 150;

    private readonly List<ChatMessage> _messages = [];

    public Guid UserId { get; private set; }

    public ChatType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public ChatStatus Status { get; private set; }

    public Guid? PostId { get; private set; }

    public DateTime? LastMessageAt { get; private set; }

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

    public ChatMessage AddUserMessage(string content, DateTime utcNow)
        => AddMessage(ChatMessageRole.User, content, utcNow);

    public ChatMessage AddAssistantMessage(
        string content,
        DateTime utcNow,
        string? model = null,
        int? inputTokens = null,
        int? outputTokens = null)
        => AddMessage(ChatMessageRole.Assistant, content, utcNow, model, inputTokens, outputTokens);

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
