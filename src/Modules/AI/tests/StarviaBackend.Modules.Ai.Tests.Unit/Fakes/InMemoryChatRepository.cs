using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Repositories;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Fakes;

internal sealed class InMemoryChatRepository : IChatRepository
{
    private readonly Dictionary<Guid, Chat> _chats = [];

    public IReadOnlyCollection<Chat> Chats => _chats.Values;

    /// <summary>How many times the handler persisted the chat. Zero means nothing was saved.</summary>
    public int UpdateCalls { get; private set; }

    public Chat Seed(
        Guid userId,
        ChatType type = ChatType.AskAgent,
        string title = "Seeded chat")
    {
        var chat = Chat.Create(userId, type, title);
        _chats.Add(chat.Id, chat);
        return chat;
    }

    public Task AddAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        _chats.Add(chat.Id, chat);
        return Task.CompletedTask;
    }

    public Task<Chat?> GetByIdAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        _chats.TryGetValue(chatId, out var chat);
        return Task.FromResult(chat);
    }

    public Task<Chat?> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var chat = _chats.Values.FirstOrDefault(c => c.PostId == postId);
        return Task.FromResult(chat);
    }

    public Task UpdateAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        UpdateCalls++;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        _chats.Remove(chat.Id);
        return Task.CompletedTask;
    }
}
