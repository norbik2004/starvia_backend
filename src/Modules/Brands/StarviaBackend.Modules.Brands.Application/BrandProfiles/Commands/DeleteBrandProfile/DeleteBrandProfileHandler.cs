using StarviaBackend.Modules.Brands.Application.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Exceptions;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.DeleteBrandProfile;

internal sealed class DeleteBrandProfileHandler(
    IBrandProfileRepository brandProfileRepository,
    IBrandAccessService access)
    : ICommandHandler<DeleteBrandProfileCommand>
{
    public async Task HandleAsync(DeleteBrandProfileCommand command, CancellationToken cancellationToken = default)
    {
        var profile = await brandProfileRepository.GetByIdAsync(command.BrandProfileId, cancellationToken)
            ?? throw new BrandProfileNotFoundException(command.BrandProfileId);

        access.EnsureAllowed(BrandAction.Delete, profile);

        await brandProfileRepository.RemoveAsync(profile, cancellationToken);
    }
}
