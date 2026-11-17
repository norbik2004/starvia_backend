using FluentValidation;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.SetBrandProfileUserPlatforms;

internal sealed class SetBrandProfileUserPlatformsValidator : AbstractValidator<SetBrandProfileUserPlatformsCommand>
{
    public SetBrandProfileUserPlatformsValidator()
    {
        RuleFor(x => x.Request.BrandProfileId).NotEmpty();
        RuleFor(x => x.Request.UserPlatforms).NotNull();
        RuleForEach(x => x.Request.UserPlatforms).ChildRules(link =>
        {
            link.RuleFor(l => l.UserPlatformId).NotEmpty();
        });
    }
}
