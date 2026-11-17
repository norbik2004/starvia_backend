using StarviaBackend.Modules.Brands.Core.BrandProfiles.Models;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.UpdateBrandProfile;

public sealed record UpdateBrandProfileCommand(UpdateBrandProfileRequest Request)
    : ICommand;

public sealed record UpdateBrandProfileRequest(
    Guid BrandProfileId,
    string Name,
    BrandBrief Brief,
    IReadOnlyList<PlatformType> Platforms,
    BrandGenerationDefaults? GenerationDefaults);
