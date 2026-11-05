using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Infrastructure.EF.ChatMessages.Configurations.Read.Models;

namespace StarviaBackend.Modules.Ai.Infrastructure.EF.ChatMessages.Configurations.Read;

internal sealed class ChatMessageReadConfiguration : IEntityTypeConfiguration<ChatMessageReadModel>
{
    public void Configure(EntityTypeBuilder<ChatMessageReadModel> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.ChatId).IsRequired();
        builder.Property(x => x.Sequence).IsRequired();
        builder.Property(x => x.Role).IsRequired();
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(ChatMessage.MaxModelLength);
        builder.Property(x => x.InputTokens);
        builder.Property(x => x.OutputTokens);
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
