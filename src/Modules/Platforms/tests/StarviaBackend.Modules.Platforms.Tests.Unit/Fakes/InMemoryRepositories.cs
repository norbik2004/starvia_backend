using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Core.Platforms.Enums;
using StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;

namespace StarviaBackend.Modules.Platforms.Tests.Unit.Fakes;

internal sealed class InMemoryPlatformRepository : IPlatformRepository
{
    public List<Platform> Platforms { get; } = [];

    public Task<Platform?> GetByIdAsync(Guid platformId, CancellationToken cancellationToken = default)
        => Task.FromResult(Platforms.FirstOrDefault(p => p.Id == platformId));

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

    public Task<UserPlatform?> GetByIdAndUserIdAsync(Guid userPlatformId, Guid UserId, CancellationToken cancellationToken = default)
        => Task.FromResult(UserPlatforms.FirstOrDefault(u => u.Id == userPlatformId && u.UserId == UserId));

    public Task AddAsync(UserPlatform userPlatform)
    {
        UserPlatforms.Add(userPlatform);
        return Task.CompletedTask;
    }
}
