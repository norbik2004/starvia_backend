using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.PostAttachments.Repositories;

internal class PostAttachmentRepository(PostWriteDbContext dbContext) : IPostAttachmentRepository
{
    public async Task AddAsync(PostAttachment postAttachment, CancellationToken cancellationToken = default)
    {
        await dbContext.PostAttachments.AddAsync(postAttachment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<PostAttachment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await dbContext.PostAttachments
            .Where(pa => pa.PostId == postId)
            .OrderBy(pa => pa.Order)
            .ThenBy(pa => pa.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<PostAttachment> postAttachments, CancellationToken cancellationToken = default)
    {
        dbContext.PostAttachments.UpdateRange(postAttachments);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PostAttachment?> GetByIdAsync(Guid postAttachmentId, CancellationToken cancellationToken = default)
    {
        return await dbContext.PostAttachments.FirstOrDefaultAsync(pa => pa.Id == postAttachmentId, cancellationToken);
    }

    public async Task RemoveAsync(PostAttachment postAttachment, CancellationToken cancellationToken = default)
    {
        dbContext.PostAttachments.Remove(postAttachment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PostAttachment postAttachment, CancellationToken cancellationToken = default)
    {
        dbContext.PostAttachments.Update(postAttachment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
