using StarviaBackend.Modules.Brands.Application.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Entities;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.CreateBrandProfile;

internal sealed class CreateBrandProfileHandler(
    IBrandProfileRepository brandProfileRepository,
    IBrandAccessService access)
    : ICommandHandler<CreateBrandProfileCommand, CreateBrandProfileResult>
{
    public async Task<CreateBrandProfileResult> HandleAsync(
        CreateBrandProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        _ = access.ScopeFor(BrandAction.Create);

        var profile = BrandProfile.Create(
            command.UserId,
            command.Request.Name.Trim(),
            command.Request.Brief,
            command.Request.GenerationDefaults,
            command.Request.Platforms);

        await brandProfileRepository.AddAsync(profile, cancellationToken);
        return new CreateBrandProfileResult(profile.Id);
    }
}
