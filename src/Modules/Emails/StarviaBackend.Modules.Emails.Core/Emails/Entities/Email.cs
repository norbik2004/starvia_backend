using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Emails.Core.Emails.Entities;

internal sealed class Email : BaseEntity
{
    public string SentTo { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public DateTime SentAt { get; private set; }

    private Email()
    {

    }

    public static Email Create(string title, string body, DateTime sentAt, string sentTo)
    {
        return new Email
        {
            Id = Guid.NewGuid(),
            Title = title,
            Body = body,
            SentAt = sentAt,
            SentTo = sentTo
        };
    }
}
