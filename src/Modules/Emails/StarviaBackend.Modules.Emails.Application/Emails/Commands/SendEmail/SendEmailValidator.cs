using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace StarviaBackend.Modules.Emails.Application.Emails.Commands.SendEmail;

internal sealed class SendEmailValidator : AbstractValidator<SendEmailCommand>
{
    public SendEmailValidator()
    {
        RuleFor(x => x.request.Email).NotEmpty()
            .EmailAddress();
        RuleFor(x => x.request.EmailType).IsInEnum();
    }
}
