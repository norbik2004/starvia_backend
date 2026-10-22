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

/// <summary>
/// The account owner refused to issue the token an e-mail needs (user gone, e-mail already
/// confirmed, ...). Permanent for this request, so it must not be retried.
/// </summary>
internal sealed class AccountTokenRejectedException(Guid userId, string rejectionCode, string reason)
    : BusinessException($"Token for user '{userId}' was rejected ({rejectionCode}): {reason}")
{
    public override string Code => "account_token_rejected";

    public string RejectionCode { get; } = rejectionCode;
}

