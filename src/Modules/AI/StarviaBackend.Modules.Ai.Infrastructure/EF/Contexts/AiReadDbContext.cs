using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Ai.Infrastructure.EF.ChatMessages.Configurations.Read;
using StarviaBackend.Modules.Ai.Infrastructure.EF.ChatMessages.Configurations.Read.Models;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Configurations.Read;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Configurations.Read.Models;

namespace StarviaBackend.Modules.Ai.Infrastructure.EF.Contexts;

internal sealed class AiReadDbContext(DbContextOptions<AiReadDbContext> options) : DbContext(options)
{
    public DbSet<ChatReadModel> Chats => Set<ChatReadModel>();
    public DbSet<ChatMessageReadModel> ChatMessages => Set<ChatMessageReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(AiWriteDbContext.Schema);

        modelBuilder.ApplyConfiguration(new ChatReadConfiguration());
        modelBuilder.ApplyConfiguration(new ChatMessageReadConfiguration());
    }

    public override int SaveChanges() => throw new InvalidOperationException("Read context is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Read context is read-only.");
}
