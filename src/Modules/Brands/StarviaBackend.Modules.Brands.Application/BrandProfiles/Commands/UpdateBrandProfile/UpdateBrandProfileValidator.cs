using FluentValidation;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Entities;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.UpdateBrandProfile;

internal sealed class UpdateBrandProfileValidator : AbstractValidator<UpdateBrandProfileCommand>
{
    public UpdateBrandProfileValidator()
    {
        RuleFor(x => x.Request.BrandProfileId).NotEmpty();
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(BrandProfile.MaxNameLength);
        RuleFor(x => x.Request.Platforms)
            .NotNull()
            .NotEmpty();
        RuleForEach(x => x.Request.Platforms).IsInEnum();
        RuleFor(x => x.Request.Platforms)
            .Must(p => p.Distinct().Count() == p.Count)
            .WithMessage("Platform types must be unique.")
            .When(x => x.Request.Platforms is not null);
    }
}
