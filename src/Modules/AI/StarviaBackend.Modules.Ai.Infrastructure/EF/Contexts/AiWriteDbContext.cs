using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Infrastructure.EF.ChatMessages.Configurations.Write;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Configurations.Write;

namespace StarviaBackend.Modules.Ai.Infrastructure.EF.Contexts;

internal sealed class AiWriteDbContext(DbContextOptions<AiWriteDbContext> options) : DbContext(options)
{
    public const string Schema = "ai";

    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfiguration(new ChatConfiguration());
        modelBuilder.ApplyConfiguration(new ChatMessageConfiguration());
    }
}
