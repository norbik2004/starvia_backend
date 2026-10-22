using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Emails.Core.Emails.Entities;
using StarviaBackend.Modules.Emails.Core.Emails.Repositories;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Repositories;

internal sealed class EmailRepository(EmailsWriteDbContext dbContext) : IEmailRepository
{
    private readonly DbSet<Email> _emails = dbContext.Emails;
    public async Task AddAsync(Email email, CancellationToken cancellationToken = default)
    {
        await _emails.AddAsync(email, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
