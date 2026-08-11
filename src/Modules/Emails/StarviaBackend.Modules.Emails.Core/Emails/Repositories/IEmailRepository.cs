using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Emails.Core.Emails.Entities;

namespace StarviaBackend.Modules.Emails.Core.Emails.Repositories;

internal interface IEmailRepository
{
    Task AddAsync(Email email, CancellationToken cancellationToken = default);
}
