using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Fakes;

internal sealed class InMemoryPostRepository : IPostRepository
{
    public List<Post> Posts { get; } = [];
    public List<Post> Updated { get; } = [];
    public List<Post> Removed { get; } = [];

    public Task AddAsync(Post post)
    {
        Posts.Add(post);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Post post)
    {
        Updated.Add(post);
        return Task.CompletedTask;
    }

    public Task<Post?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken)
        => Task.FromResult(Posts.FirstOrDefault(p => p.Id == postId));

    public Task RemovePostAsync(Post post, CancellationToken cancellationToken)
    {
        Posts.Remove(post);
        Removed.Add(post);
        return Task.CompletedTask;
    }

    /// <summary>Seeds a post "created" by the given user, as the audit interceptor would.</summary>
    public Post Seed(Guid ownerId, string title = "Seeded post")
    {
        var post = Post.Create(title);
        post.CreatedBy = ownerId.ToString();
        Posts.Add(post);
        return post;
    }
}
