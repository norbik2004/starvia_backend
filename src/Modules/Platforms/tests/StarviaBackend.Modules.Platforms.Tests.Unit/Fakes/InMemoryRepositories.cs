using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Shared.Abstractions.Platforms;
using StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;

namespace StarviaBackend.Modules.Platforms.Tests.Unit.Fakes;

internal sealed class InMemoryPlatformRepository : IPlatformRepository
{
    public List<Platform> Platforms { get; } = [];

    public Task<Platform?> GetByIdAsync(Guid platformId, CancellationToken cancellationToken = default)
        => Task.FromResult(Platforms.FirstOrDefault(p => p.Id == platformId));

    public Task<Platform?> GetByTypeAsync(PlatformType platformType, CancellationToken cancellationToken = default)
        => Task.FromResult(Platforms.FirstOrDefault(p => p.PlatformType == platformType));

    public Platform Seed(PlatformType type = PlatformType.Facebook, bool isOnline = true)
    {
        var platform = Platform.Create(type, isOnline);
        Platforms.Add(platform);
        return platform;
    }
}

internal sealed class InMemoryUserPlatformRepository : IUserPlatformRepository
{
    public List<UserPlatform> UserPlatforms { get; } = [];

    public Task AddAsync(UserPlatform userPlatform)
    {
        UserPlatforms.Add(userPlatform);
        return Task.CompletedTask;
    }

    public Task<UserPlatform?> GetByIdAsync(Guid userPlatformId, CancellationToken cancellationToken = default)
        => Task.FromResult(UserPlatforms.FirstOrDefault(u => u.Id == userPlatformId));
}
