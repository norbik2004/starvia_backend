using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.GenerateEmailConfirmationToken;

internal sealed class GenerateEmailConfirmationTokenValidator : AbstractValidator<GenerateEmailConfirmationTokenCommand>
{
    public GenerateEmailConfirmationTokenValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();  
    }
}
