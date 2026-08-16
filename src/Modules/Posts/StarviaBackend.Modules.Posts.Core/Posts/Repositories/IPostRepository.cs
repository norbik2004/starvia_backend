using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;

namespace StarviaBackend.Modules.Posts.Core.Posts.Repositories;

internal interface IPostRepository
{
    Task AddAsync(Post post);
}
