using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Configurations.Read.Models;

namespace StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Configurations.Read;

internal sealed class ChatReadConfiguration : IEntityTypeConfiguration<ChatReadModel>
{
    public void Configure(EntityTypeBuilder<ChatReadModel> builder)
    {
        builder.ToTable("Chats");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(Chat.MaxTitleLength);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.PostId);
        builder.Property(x => x.LastMessageAt);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany(x => x.Messages)
            .WithOne()
            .HasForeignKey(x => x.ChatId);
    }
}
