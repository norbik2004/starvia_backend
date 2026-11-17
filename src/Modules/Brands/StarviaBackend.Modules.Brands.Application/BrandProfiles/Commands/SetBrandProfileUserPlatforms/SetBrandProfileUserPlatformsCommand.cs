using StarviaBackend.Modules.Brands.Application.BrandProfiles;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.SetBrandProfileUserPlatforms;

public sealed record SetBrandProfileUserPlatformsCommand(SetBrandProfileUserPlatformsRequest Request)
    : ICommand;

public sealed record SetBrandProfileUserPlatformsRequest(
    Guid BrandProfileId,
    IReadOnlyList<BrandProfileUserPlatformLink> UserPlatforms);
