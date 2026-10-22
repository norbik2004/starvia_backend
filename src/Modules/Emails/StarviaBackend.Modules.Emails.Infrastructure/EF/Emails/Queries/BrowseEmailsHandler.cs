using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Emails.Application.Emails.Queries.BrowseEmails;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Queries;

internal sealed class BrowseEmailsHandler(EmailsReadDbContex dbContext)
    : IQueryHandler<BrowseEmailsQuery, PagedResult<EmailDto>>
{
    public async Task<PagedResult<EmailDto>> HandleAsync(BrowseEmailsQuery query, CancellationToken cancellationToken = default)
    {
        var total = await dbContext.Emails.LongCountAsync(cancellationToken);
        if (total == 0)
        {
            return PagedResult<EmailDto>.Empty(query.Page, query.PageSize);
        }

        var items = await dbContext.Emails
            .OrderBy(u => u.SentAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new EmailDto(u.Id, u.Title, u.SentTo, u.Body, u.SentAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<EmailDto>(items, query.Page, query.PageSize, total);
    }
}
