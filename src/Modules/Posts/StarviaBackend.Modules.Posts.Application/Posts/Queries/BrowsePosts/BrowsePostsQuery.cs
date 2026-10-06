using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Application.Posts.Queries.BrowsePosts;

internal sealed record BrowsePostsQuery(int Page = 1, int PageSize = 20) : IPagedQuery<PostDto>
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;
}

public sealed record PostDto(Guid Id, string Title, DateTime CreatedAt, string CreatedBy);
