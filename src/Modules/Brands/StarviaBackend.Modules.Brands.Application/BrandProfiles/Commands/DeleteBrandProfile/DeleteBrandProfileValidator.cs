using FluentValidation;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.DeleteBrandProfile;

internal sealed class DeleteBrandProfileValidator : AbstractValidator<DeleteBrandProfileCommand>
{
    public DeleteBrandProfileValidator()
    {
        RuleFor(x => x.BrandProfileId).NotEmpty();
    }
}
