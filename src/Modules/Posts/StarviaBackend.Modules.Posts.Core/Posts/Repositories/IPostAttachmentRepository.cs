using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;

namespace StarviaBackend.Modules.Posts.Core.Posts.Repositories;

internal interface IPostAttachmentRepository
{
    Task AddAsync(PostAttachment postAttachment, CancellationToken cancellationToken = default);
    Task UpdateAsync(PostAttachment postAttachment, CancellationToken cancellationToken = default);
    Task RemoveAsync(PostAttachment postAttachment, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<PostAttachment> postAttachments, CancellationToken cancellationToken = default);
    Task<List<PostAttachment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<PostAttachment?> GetByIdAsync(Guid postAttachmentId, CancellationToken cancellationToken = default);
}
