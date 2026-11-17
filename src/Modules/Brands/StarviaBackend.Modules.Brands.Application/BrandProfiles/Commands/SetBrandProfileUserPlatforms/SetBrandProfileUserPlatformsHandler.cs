using StarviaBackend.Modules.Brands.Application.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Exceptions;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.SetBrandProfileUserPlatforms;

internal sealed class SetBrandProfileUserPlatformsHandler(
    IBrandProfileRepository brandProfileRepository,
    IBrandAccessService access)
    : ICommandHandler<SetBrandProfileUserPlatformsCommand>
{
    public async Task HandleAsync(
        SetBrandProfileUserPlatformsCommand command,
        CancellationToken cancellationToken = default)
    {
        var profile = await brandProfileRepository.GetByIdAsync(command.Request.BrandProfileId, cancellationToken)
            ?? throw new BrandProfileNotFoundException(command.Request.BrandProfileId);

        access.EnsureAllowed(BrandAction.LinkUserPlatforms, profile);

        var links = command.Request.UserPlatforms
            .Select(l => (l.UserPlatformId, l.IsDefault))
            .ToList();

        var defaultUserPlatformIds = links
            .Where(l => l.IsDefault)
            .Select(l => l.UserPlatformId)
            .ToList();

        if (defaultUserPlatformIds.Count > 0)
        {
            await brandProfileRepository.ClearDefaultsForUserPlatformsAsync(
                defaultUserPlatformIds,
                profile.Id,
                cancellationToken);
        }

        profile.ReplaceUserPlatformLinks(links);
        await brandProfileRepository.UpdateAsync(profile, cancellationToken);
    }
}
