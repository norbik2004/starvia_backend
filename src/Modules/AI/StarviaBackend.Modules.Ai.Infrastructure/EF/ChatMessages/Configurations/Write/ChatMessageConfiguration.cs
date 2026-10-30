using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;

namespace StarviaBackend.Modules.Ai.Infrastructure.EF.ChatMessages.Configurations.Write;

internal sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.ChatId).IsRequired();
        builder.Property(x => x.Sequence).IsRequired();
        builder.Property(x => x.Role).IsRequired();
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(ChatMessage.MaxModelLength);
        builder.Property(x => x.InputTokens);
        builder.Property(x => x.OutputTokens);
        builder.Property(x => x.CreatedAt).IsRequired();

        // Keeps the history ordered and rejects two messages racing for the same position in a chat.
        builder.HasIndex(x => new { x.ChatId, x.Sequence }).IsUnique();
    }
}
