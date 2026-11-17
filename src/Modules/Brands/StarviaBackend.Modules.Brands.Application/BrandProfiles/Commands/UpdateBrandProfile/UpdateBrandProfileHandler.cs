using StarviaBackend.Modules.Brands.Application.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Exceptions;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.UpdateBrandProfile;

internal sealed class UpdateBrandProfileHandler(
    IBrandProfileRepository brandProfileRepository,
    IBrandAccessService access)
    : ICommandHandler<UpdateBrandProfileCommand>
{
    public async Task HandleAsync(UpdateBrandProfileCommand command, CancellationToken cancellationToken = default)
    {
        var profile = await brandProfileRepository.GetByIdAsync(command.Request.BrandProfileId, cancellationToken)
            ?? throw new BrandProfileNotFoundException(command.Request.BrandProfileId);

        access.EnsureAllowed(BrandAction.Update, profile);

        profile.Update(
            command.Request.Name.Trim(),
            command.Request.Brief,
            command.Request.GenerationDefaults,
            command.Request.Platforms);

        await brandProfileRepository.UpdateAsync(profile, cancellationToken);
    }
}
