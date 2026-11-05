using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Ai.Application.Chats.Queries.BrowseChats;

/// <summary>
/// Lists the chats the caller is allowed to browse (see the chat access policy - today only their own).
/// The filters below narrow that set further.
/// </summary>
/// <param name="Type">Only chats of this kind, or all kinds when omitted.</param>
/// <param name="Status">Only active or only archived chats, or both when omitted.</param>
/// <param name="Search">Case-insensitive text the chat title must contain.</param>
/// <param name="PostId">Only chats linked to this post.</param>
/// <param name="OwnerId">Only chats of this user; matters only if the policy lets the caller see other users' chats.</param>
/// <param name="SortBy">Sort column, most recently active first by default.</param>
/// <param name="Descending">Sort direction.</param>
internal sealed record BrowseChatsQuery(
    int Page = 1,
    int PageSize = 20,
    ChatType? Type = null,
    ChatStatus? Status = null,
    string? Search = null,
    Guid? PostId = null,
    Guid? OwnerId = null,
    ChatSortBy SortBy = ChatSortBy.LastActivity,
    bool Descending = true) : IPagedQuery<ChatDto>
{
    public const int MaxSearchLength = 100;

    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;

    public string? Search { get; init; } = NormalizeSearch(Search);

    private static string? NormalizeSearch(string? search)
    {
        var trimmed = search?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        return trimmed.Length > MaxSearchLength ? trimmed[..MaxSearchLength] : trimmed;
    }
}

public enum ChatSortBy
{
    /// <summary>The last message, or the moment of creation for a chat without messages yet.</summary>
    LastActivity,
    CreatedAt,
    Title,
}

public sealed record ChatDto(
    Guid Id,
    ChatType Type,
    string Title,
    ChatStatus Status,
    Guid? PostId,
    DateTime? LastMessageAt,
    DateTime CreatedAt,
    Guid OwnerId);
