using StarviaBackend.Shared.Abstractions.Email;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Emails.Application.Emails.Queries.BrowseEmails;

public enum EmailSortBy
{
    SentAt,
    SentTo,
    Title,
}

/// <summary>
/// Browses the sent-mail log (admin only - the access policy decides). <paramref name="Search"/> matches the
/// recipient or the title, case-insensitively, with wildcards treated literally. <paramref name="From"/> is
/// inclusive and <paramref name="To"/> exclusive, both on the sent time.
/// </summary>
public sealed record BrowseEmailsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    EmailType? Type = null,
    DateTime? From = null,
    DateTime? To = null,
    EmailSortBy SortBy = EmailSortBy.SentAt,
    bool Descending = false) : IPagedQuery<EmailDto>
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;
}

public sealed record EmailDto(Guid Id, string Title, string SentTo, string Body, DateTime SentAt, EmailType Type = default);