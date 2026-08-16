using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Repositories;

internal class PostRepository() : IPostRepository
{
    public Task AddAsync(Post post)
    {
        throw new NotImplementedException();
    }
}
