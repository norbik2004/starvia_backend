using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;

namespace StarviaBackend.Modules.Posts.Core.Posts.Repositories;

internal interface IPostRepository
{
    Task AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task<Post?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken);
}
