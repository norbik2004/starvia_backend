using System;
using System.Collections.Generic;
using System.Text;

using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Configurations.Read.Models;

internal sealed class EmailReadModel
{
    public Guid Id { get; init; }
    public string SentTo { get; init; }
    public string Title { get; init; }
    public string Body { get; init; }
    public EmailType Type { get; init; }
    public DateTime SentAt { get; init; }
}
