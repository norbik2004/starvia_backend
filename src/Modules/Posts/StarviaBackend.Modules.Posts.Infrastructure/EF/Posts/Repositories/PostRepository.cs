using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Repositories;

internal class PostRepository(PostWriteDbContext dbContext) : IPostRepository
{
    private readonly DbSet<Post> _posts = dbContext.Posts;
    public async Task AddAsync(Post post)
    {
        await _posts.AddAsync(post);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Post?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken)
    {
        return await _posts.Include(p => p.PostPublications)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);
    }

    public async Task UpdateAsync(Post post)
    {
        _posts.Update(post);
        await dbContext.SaveChangesAsync();
    }
}
