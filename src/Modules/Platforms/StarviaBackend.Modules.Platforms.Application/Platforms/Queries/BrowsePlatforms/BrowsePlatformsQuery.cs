using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Platforms.Core.Platforms.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Platforms.Application.Platforms.Queries.BrowsePlatforms;

public sealed record BrowsePlatformsQuery(int Page = 1, int PageSize = 20) : IPagedQuery<PlatformDto>
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;
}

public sealed record PlatformDto(Guid Id, PlatformType PlatformType, bool IsOnline);
