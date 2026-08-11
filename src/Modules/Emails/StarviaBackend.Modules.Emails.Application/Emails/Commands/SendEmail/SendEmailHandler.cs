using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Emails.Application.Emails.Commands.SendEmail;

internal sealed class SendEmailHandler()
    : ICommandHandler<SendEmailCommand, SendEmailResult>
{
    public Task<SendEmailResult> HandleAsync(SendEmailCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
