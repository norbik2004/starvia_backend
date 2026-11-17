namespace StarviaBackend.Modules.Brands.Core.BrandProfiles.Models;

public sealed record BrandBrief(
    BrandCompany? Company,
    IReadOnlyList<BrandOffering>? Offerings,
    BrandTargetAudience? TargetAudience,
    BrandIdentity? BrandIdentity,
    IReadOnlyList<string>? MarketingGoals);

public sealed record BrandCompany(string? Name, string? Industry, string? Description);

public sealed record BrandOffering(string? Name, string? Description);

public sealed record BrandTargetAudience(string? Type, IReadOnlyList<string>? PainPoints);

public sealed record BrandIdentity(string? ToneOfVoice, string? Style, IReadOnlyList<string>? WordsToAvoid);

public sealed record BrandGenerationDefaults(
    string? PostType,
    string? Goal,
    string? Language);
