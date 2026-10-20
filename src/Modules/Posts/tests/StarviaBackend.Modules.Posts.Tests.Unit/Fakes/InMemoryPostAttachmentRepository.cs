using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Fakes;

internal sealed class InMemoryPostAttachmentRepository : IPostAttachmentRepository
{
    public List<PostAttachment> Attachments { get; } = [];
    public List<PostAttachment> Removed { get; } = [];
    public int UpdateRangeCalls { get; private set; }

    public Task AddAsync(PostAttachment postAttachment, CancellationToken cancellationToken = default)
    {
        Attachments.Add(postAttachment);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(PostAttachment postAttachment, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RemoveAsync(PostAttachment postAttachment, CancellationToken cancellationToken = default)
    {
        Attachments.Remove(postAttachment);
        Removed.Add(postAttachment);
        return Task.CompletedTask;
    }

    public Task UpdateRangeAsync(IEnumerable<PostAttachment> postAttachments, CancellationToken cancellationToken = default)
    {
        UpdateRangeCalls++;
        return Task.CompletedTask;
    }

    /// <summary>Returns a fresh list (handlers mutate it), ordered like the real repository.</summary>
    public Task<List<PostAttachment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
        => Task.FromResult(Attachments.Where(a => a.PostId == postId).OrderBy(a => a.Order).ToList());

    public Task<PostAttachment?> GetByIdAsync(Guid postAttachmentId, CancellationToken cancellationToken = default)
        => Task.FromResult(Attachments.FirstOrDefault(a => a.Id == postAttachmentId));

    public PostAttachment Seed(Guid postId, int order)
    {
        var attachment = PostAttachment.Create(postId, Guid.NewGuid(), order);
        Attachments.Add(attachment);
        return attachment;
    }
}
