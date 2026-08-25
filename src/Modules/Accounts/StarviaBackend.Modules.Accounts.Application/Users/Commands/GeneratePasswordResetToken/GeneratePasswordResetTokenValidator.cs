using FluentValidation;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.GeneratePasswordResetToken;

internal sealed class GeneratePasswordResetTokenValidator : AbstractValidator<GeneratePasswordResetTokenCommand>
{
    public GeneratePasswordResetTokenValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
