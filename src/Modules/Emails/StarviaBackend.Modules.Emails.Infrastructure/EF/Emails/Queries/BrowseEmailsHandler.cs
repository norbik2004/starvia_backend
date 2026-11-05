using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Emails.Application.Emails.Access;
using StarviaBackend.Modules.Emails.Application.Emails.Queries.BrowseEmails;
using StarviaBackend.Modules.Emails.Core.Emails.Access;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Configurations.Read.Models;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Queries;

internal sealed class BrowseEmailsHandler(EmailsReadDbContex dbContext, IEmailAccessService access)
    : IQueryHandler<BrowseEmailsQuery, PagedResult<EmailDto>>
{
    public async Task<PagedResult<EmailDto>> HandleAsync(BrowseEmailsQuery query, CancellationToken cancellationToken = default)
    {
        // Admin-only: a non-admin gets 403 here rather than an empty page that looks like "no e-mails".
        // Stored e-mails have no owner, so an unrestricted scope is the only non-empty one there is.
        access.RequireScopeFor(EmailAction.BrowseEmails);

        var emails = dbContext.Emails.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{EscapeLike(query.Search.Trim())}%";
            emails = emails.Where(e =>
                Microsoft.EntityFrameworkCore.EF.Functions.ILike(e.SentTo, pattern, "\\")
                || Microsoft.EntityFrameworkCore.EF.Functions.ILike(e.Title, pattern, "\\"));
        }

        if (query.Type is { } type)
        {
            emails = emails.Where(e => e.Type == type);
        }

        if (query.From is { } from)
        {
            emails = emails.Where(e => e.SentAt >= from);
        }

        if (query.To is { } to)
        {
            emails = emails.Where(e => e.SentAt < to);
        }

        var total = await emails.LongCountAsync(cancellationToken);
        var skip = (long)(query.Page - 1) * query.PageSize;
        if (total == 0 || skip >= total)
        {
            return new PagedResult<EmailDto>([], query.Page, query.PageSize, total);
        }

        var items = await Order(emails, query)
            .Skip((int)skip)
            .Take(query.PageSize)
            .Select(e => new EmailDto(e.Id, e.Title, e.SentTo, e.Body, e.SentAt, e.Type))
            .ToListAsync(cancellationToken);

        return new PagedResult<EmailDto>(items, query.Page, query.PageSize, total);
    }

    // Id is the tie-breaker so paging stays stable when the sort key repeats.
    private static IOrderedQueryable<EmailReadModel> Order(IQueryable<EmailReadModel> emails, BrowseEmailsQuery query) =>
        (query.SortBy, query.Descending) switch
        {
            (EmailSortBy.SentTo, false) => emails.OrderBy(e => e.SentTo).ThenBy(e => e.Id),
            (EmailSortBy.SentTo, true) => emails.OrderByDescending(e => e.SentTo).ThenBy(e => e.Id),
            (EmailSortBy.Title, false) => emails.OrderBy(e => e.Title).ThenBy(e => e.Id),
            (EmailSortBy.Title, true) => emails.OrderByDescending(e => e.Title).ThenBy(e => e.Id),
            (_, true) => emails.OrderByDescending(e => e.SentAt).ThenBy(e => e.Id),
            _ => emails.OrderBy(e => e.SentAt).ThenBy(e => e.Id),
        };

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}