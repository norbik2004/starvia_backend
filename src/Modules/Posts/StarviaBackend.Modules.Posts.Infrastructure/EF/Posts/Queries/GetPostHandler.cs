using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Queries.BrowsePostAttachments;
using StarviaBackend.Modules.Posts.Application.Posts.Queries.GetPost;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Queries;

internal sealed class GetPostHandler(PostReadDbContext dbContext) : IQueryHandler<GetPostQuery, PostLongDto>
{
    public async Task<PostLongDto> HandleAsync(GetPostQuery query, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.Posts
            .Include(p => p.PostAttachments)
            .FirstOrDefaultAsync(p => p.Id == query.PostId, cancellationToken)
            ?? throw new PostNotFoundException(query.PostId);

        if (item.CreatedBy != query.UserId)
        {
            throw new PostNotOwnedException(query.PostId);
        }

        ICollection<PostAttachmentDto> postAttachments = item.PostAttachments
            .Select(pa => new PostAttachmentDto(pa.Id, pa.FileId, pa.CreatedAt, pa.Order))
            .ToList()
            .AsReadOnly();

        return new PostLongDto(item.Id, item.Title, item.Body ?? "", item.Status, item.CreatedAt, item.CreatedBy, item.LastModifiedAt, postAttachments);
    }
}
