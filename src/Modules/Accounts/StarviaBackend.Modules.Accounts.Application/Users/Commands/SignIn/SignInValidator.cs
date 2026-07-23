using FluentValidation;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.SignIn;

internal sealed class SignInValidator : AbstractValidator<SignInCommand>
{
    public SignInValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
