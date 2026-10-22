using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Emails.Application.Emails.Queries.BrowseEmails;

public sealed record BrowseEmailsQuery(int Page = 1, int PageSize = 20) : IPagedQuery<EmailDto>
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;
}

public sealed record EmailDto(Guid Id, string Title, string SentTo, string Body, DateTime SentAt);
