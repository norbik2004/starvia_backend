using FluentValidation;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.RequestPasswordReset;

internal sealed class RequestPasswordResetValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
