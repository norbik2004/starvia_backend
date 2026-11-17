using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Brands.Integrations.External;

internal sealed record BrandProfileApiResponse(
    Guid Id,
    string Name,
    BrandBriefApiResponse Brief,
    BrandGenerationDefaultsApiResponse? GenerationDefaults,
    IReadOnlyList<PlatformType> Platforms,
    DateTime CreatedAt,
    Guid OwnerId);

internal sealed record BrandBriefApiResponse(
    BrandCompanyApiResponse? Company,
    IReadOnlyList<BrandOfferingApiResponse>? Offerings,
    BrandTargetAudienceApiResponse? TargetAudience,
    BrandIdentityApiResponse? BrandIdentity,
    IReadOnlyList<string>? MarketingGoals);

internal sealed record BrandCompanyApiResponse(string? Name, string? Industry, string? Description);

internal sealed record BrandOfferingApiResponse(string? Name, string? Description);

internal sealed record BrandTargetAudienceApiResponse(string? Type, IReadOnlyList<string>? PainPoints);

internal sealed record BrandIdentityApiResponse(string? ToneOfVoice, string? Style, IReadOnlyList<string>? WordsToAvoid);

internal sealed record BrandGenerationDefaultsApiResponse(
    string? PostType,
    string? Goal,
    string? Language);
