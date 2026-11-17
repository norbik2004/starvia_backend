using StarviaBackend.Modules.Brands.Core.BrandProfiles.Models;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.CreateBrandProfile;

public sealed record CreateBrandProfileCommand(CreateBrandProfileRequest Request, Guid UserId)
    : ICommand<CreateBrandProfileResult>;

public sealed record CreateBrandProfileRequest(
    string Name,
    BrandBrief Brief,
    IReadOnlyList<PlatformType> Platforms,
    BrandGenerationDefaults? GenerationDefaults);

public sealed record CreateBrandProfileResult(Guid BrandProfileId);
