using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Core.Chats.Repositories;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Repositories;

internal sealed class ChatRepository(AiWriteDbContext dbContext) : IChatRepository
{
    public async Task AddAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        await dbContext.Chats.AddAsync(chat, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Chat?> GetByIdAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        return dbContext.Chats
            .Include(c => c.Messages.OrderBy(m => m.Sequence))
            .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);
    }

    public async Task UpdateAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        // The chat must come from this repository (or be added through it), so it is already tracked and
        // change detection picks up only the changed columns and inserts any messages added to it.
        // DbSet.Update is avoided on purpose: it would rewrite every column of the chat and, for an
        // untracked chat, mark freshly added messages as Modified, so the save would try to UPDATE rows
        // that do not exist yet.
        if (dbContext.Entry(chat).State == EntityState.Detached)
        {
            throw new InvalidOperationException(
                $"Chat '{chat.Id}' is not tracked. Load it through {nameof(IChatRepository)} before updating it.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        dbContext.Chats.Remove(chat);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
