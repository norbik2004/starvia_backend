using FluentValidation;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Commands.AddUserPlatform;

namespace StarviaBackend.Modules.Platforms.Application.UserPlatforms.Commands.AddUserPlatform;

internal sealed class AddUserPlatformValidator : AbstractValidator<AddUserPlatformCommand>
{
    public AddUserPlatformValidator()
    {
        RuleFor(x => x.request.AccountUserName).NotEmpty()
            .MinimumLength(3)
            .MaximumLength(25)
            .Matches(@"^[a-zA-Z]+$");

        RuleFor(x => x.request.AccountComment)
            .MaximumLength(250);

        RuleFor(x => x.request.PlatformId).NotEmpty();
    }
}
