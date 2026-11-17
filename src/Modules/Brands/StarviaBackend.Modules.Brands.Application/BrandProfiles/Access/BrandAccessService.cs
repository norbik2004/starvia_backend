using StarviaBackend.Modules.Brands.Core.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Entities;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Exceptions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Access;

internal interface IBrandAccessService
{
    AccessScope ScopeFor(BrandAction action);

    void EnsureAllowed(BrandAction action, Guid brandProfileId, string? ownerId);

    void EnsureAllowed(BrandAction action, BrandProfile profile) =>
        EnsureAllowed(action, profile.Id, profile.UserId.ToString());
}

internal sealed class BrandAccessService(IContext context) : IBrandAccessService
{
    public AccessScope ScopeFor(BrandAction action) =>
        BrandAccessPolicy.ScopeFor(action, AccessActor.From(context.Identity));

    public void EnsureAllowed(BrandAction action, Guid brandProfileId, string? ownerId)
    {
        if (!ScopeFor(action).Allows(ownerId))
        {
            throw new BrandProfileAccessDeniedException(brandProfileId, action);
        }
    }
}
