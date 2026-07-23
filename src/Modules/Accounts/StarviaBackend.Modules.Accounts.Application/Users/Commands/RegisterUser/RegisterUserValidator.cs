using FluentValidation;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.RegisterUser;

internal sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(256);
    }
}
