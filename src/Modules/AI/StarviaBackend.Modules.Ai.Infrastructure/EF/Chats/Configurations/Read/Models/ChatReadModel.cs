using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Infrastructure.EF.ChatMessages.Configurations.Read.Models;

namespace StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Configurations.Read.Models;

internal sealed class ChatReadModel
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public ChatType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public ChatStatus Status { get; init; }
    public Guid? PostId { get; init; }
    public DateTime? LastMessageAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public ICollection<ChatMessageReadModel> Messages { get; init; } = [];
}
