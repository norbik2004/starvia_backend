using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Application.Posts.Queries.GetPost;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Queries;

internal sealed class GetPostHandler(PostReadDbContext dbContext) : IQueryHandler<GetPostQuery, PostLongDto>
{
    public async Task<PostLongDto> HandleAsync(GetPostQuery query, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == query.PostId)
            ?? throw new PostNotFoundException(query.PostId);

        if (item.CreatedBy != query.UserId)
        {
            throw new UnauthorizedAccessException("You cannot see this post.");
        }

        return new PostLongDto(item.Id, item.Title, item.Body ?? "", item.Status, item.CreatedAt, item.CreatedBy, item.LastModifiedAt);
    }
}
