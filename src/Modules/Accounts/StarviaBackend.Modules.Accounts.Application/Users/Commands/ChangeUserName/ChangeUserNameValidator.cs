using FluentValidation;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.ChangeUserName;

internal sealed class ChangeUserNameValidator : AbstractValidator<ChangeUserNameCommand>
{
    public ChangeUserNameValidator()
    {
        RuleFor(x => x.UserName).NotEmpty()
            .MinimumLength(3)
            .MaximumLength(25)
            .Matches(@"^[a-zA-Z]+$");

        RuleFor(x => x.UserId).NotEmpty();
    }
}
