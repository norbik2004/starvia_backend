using StarviaBackend.Modules.Ai.Core.Chats.Entities;

namespace StarviaBackend.Modules.Ai.Core.Chats.Repositories;

internal interface IChatRepository
{
    Task AddAsync(Chat chat, CancellationToken cancellationToken = default);
    Task<Chat?> GetByIdAsync(Guid chatId, CancellationToken cancellationToken = default);
    Task<Chat?> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Chat chat, CancellationToken cancellationToken = default);
    Task RemoveAsync(Chat chat, CancellationToken cancellationToken = default);
}
