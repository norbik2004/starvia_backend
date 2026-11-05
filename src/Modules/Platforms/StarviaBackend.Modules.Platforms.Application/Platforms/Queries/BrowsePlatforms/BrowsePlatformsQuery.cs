using StarviaBackend.Modules.Platforms.Core.Platforms.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Platforms.Application.Platforms.Queries.BrowsePlatforms;

/// <summary>Lists the (public) catalogue of supported platforms, ordered by type.</summary>
/// <param name="PlatformType">Only this platform.</param>
/// <param name="IsOnline">Only platforms that are (or are not) currently online.</param>
public sealed record BrowsePlatformsQuery(
    int Page = 1,
    int PageSize = 20,
    PlatformType? PlatformType = null,
    bool? IsOnline = null) : IPagedQuery<PlatformDto>
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;
}

public sealed record PlatformDto(Guid Id, PlatformType PlatformType, bool IsOnline);
