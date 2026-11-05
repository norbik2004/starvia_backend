using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Application.Posts.Access;
using StarviaBackend.Modules.Posts.Core.Posts.Access;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Queries;

internal static class PostReadAccessExtensions
{
    /// <summary>
    /// For queries over resources of a single post (attachments, publications): loads the post's
    /// owner, then asks the access policy whether the caller may do <paramref name="action"/>.
    /// </summary>
    /// <exception cref="PostNotFoundException">The post does not exist.</exception>
    /// <exception cref="PostAccessDeniedException">The caller may not do <paramref name="action"/> on it.</exception>
    public static async Task EnsurePostAccessAsync(
        this PostReadDbContext dbContext,
        IPostAccessService access,
        PostAction action,
        Guid postId,
        CancellationToken cancellationToken)
    {
        var post = await dbContext.Posts
            .AsNoTracking()
            .Where(p => p.Id == postId)
            .Select(p => new { p.CreatedBy })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new PostNotFoundException(postId);

        access.EnsureAllowed(action, postId, post.CreatedBy);
    }
}
