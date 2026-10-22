using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.PostsPublications.Repositories;

internal class PostPublicationRepository(PostWriteDbContext dbContext) : IPostPublicationRepository
{
    private readonly DbSet<PostPublication> _postPublications = dbContext.PostPublications;
    public async Task AddAsync(PostPublication postPublication)
    {
        await _postPublications.AddAsync(postPublication);
        await dbContext.SaveChangesAsync();
    }
}
