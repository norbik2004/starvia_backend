using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Emails.Core.Emails.Exceptions;

internal sealed class EmailNotSentException(string email)
    : BusinessException($"Email to '{email}' was not sent.")
{
    public override string Code => "email_not_sent";
}

