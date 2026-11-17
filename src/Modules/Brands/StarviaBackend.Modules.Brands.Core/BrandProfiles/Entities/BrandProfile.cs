using StarviaBackend.Modules.Brands.Core.BrandProfiles.Exceptions;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Models;
using StarviaBackend.Shared.Abstractions.Domain;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Brands.Core.BrandProfiles.Entities;

internal sealed class BrandProfile : AuditableEntity
{
    public const int MaxNameLength = 100;

    private readonly List<BrandProfileUserPlatform> _userPlatforms = [];
    private readonly List<BrandProfilePlatform> _platforms = [];

    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public BrandBrief Brief { get; private set; } = null!;
    public BrandGenerationDefaults? GenerationDefaults { get; private set; }
    public IReadOnlyCollection<BrandProfileUserPlatform> UserPlatforms => _userPlatforms.AsReadOnly();
    public IReadOnlyCollection<BrandProfilePlatform> Platforms => _platforms.AsReadOnly();

    private BrandProfile()
    {
    }

    public static BrandProfile Create(
        Guid userId,
        string name,
        BrandBrief brief,
        BrandGenerationDefaults? generationDefaults,
        IReadOnlyList<PlatformType> platforms)
    {
        var profile = new BrandProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Brief = brief,
            GenerationDefaults = generationDefaults,
        };

        profile.ReplacePlatforms(platforms);
        return profile;
    }

    public void Update(
        string name,
        BrandBrief brief,
        BrandGenerationDefaults? generationDefaults,
        IReadOnlyList<PlatformType> platforms)
    {
        Name = name;
        Brief = brief;
        GenerationDefaults = generationDefaults;
        ReplacePlatforms(platforms);
    }

    public void ReplacePlatforms(IReadOnlyList<PlatformType> platforms)
    {
        var distinct = platforms.Distinct().ToList();
        if (distinct.Count != platforms.Count)
        {
            var duplicate = platforms
                .GroupBy(p => p)
                .First(g => g.Count() > 1)
                .Key;
            throw new DuplicatePlatformTypeException(duplicate);
        }

        _platforms.Clear();
        foreach (var platformType in distinct)
        {
            _platforms.Add(BrandProfilePlatform.Create(Id, platformType));
        }
    }

    public void ReplaceUserPlatformLinks(IReadOnlyList<(Guid UserPlatformId, bool IsDefault)> links)
    {
        var distinctIds = links.Select(l => l.UserPlatformId).Distinct().ToList();
        if (distinctIds.Count != links.Count)
        {
            var duplicate = links
                .GroupBy(l => l.UserPlatformId)
                .First(g => g.Count() > 1)
                .Key;
            throw new DuplicateUserPlatformLinkException(duplicate);
        }

        _userPlatforms.Clear();
        foreach (var (userPlatformId, isDefault) in links)
        {
            _userPlatforms.Add(BrandProfileUserPlatform.Create(Id, userPlatformId, isDefault));
        }
    }
}
