using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Ai.Application.Chats.Queries.BrowseChats;

/// <summary>Filters and paging a client may send. The owner is never part of it - it comes from the identity.</summary>
/// <param name="Type">Only chats of this kind, or all kinds when omitted.</param>
/// <param name="Status">Only active or only archived chats, or both when omitted.</param>
internal sealed record BrowseChatsRequest(ChatType? Type = null, ChatStatus? Status = null, int Page = 1, int PageSize = 20);

internal sealed record BrowseChatsQuery(
    Guid UserId,
    ChatType? Type = null,
    ChatStatus? Status = null,
    int Page = 1,
    int PageSize = 20) : IPagedQuery<ChatDto>
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;
}

public sealed record ChatDto(
    Guid Id,
    ChatType Type,
    string Title,
    ChatStatus Status,
    Guid? PostId,
    DateTime? LastMessageAt,
    DateTime CreatedAt);
