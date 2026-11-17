using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Domain;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Posts.Core.Posts.Entities;

internal sealed class Post : BaseEntity, IAuditable
{
    private readonly List<PostPublication> _postPublications = [];
    private readonly List<PostAttachment> _postAttachments = [];

    public string Title { get; private set; }
    public string? Body { get; private set; }
    public PostStatus Status { get; private set; }
    public IReadOnlyList<PlatformType> Platforms { get; private set; } = [];

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public IReadOnlyCollection<PostPublication> PostPublications => _postPublications.AsReadOnly();
    public IReadOnlyCollection<PostAttachment> PostAttachments => _postAttachments.AsReadOnly();

    private Post()
    {
    }

    public static Post Create(string title, IReadOnlyList<PlatformType> platforms)
    {
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = title,
            Status = PostStatus.Draft,
        };

        post.ReplacePlatforms(platforms);
        return post;
    }

    public void UpdateStatus(PostStatus status) => Status = status;

    public void Update(string title, string? body, PostStatus status, IReadOnlyList<PlatformType> platforms)
    {
        Title = title;
        Body = body;
        Status = status;
        ReplacePlatforms(platforms);
    }

    public void ReplacePlatforms(IReadOnlyList<PlatformType> platforms)
    {
        var distinct = platforms.Distinct().ToList();
        if (distinct.Count != platforms.Count)
        {
            var duplicate = platforms
                .GroupBy(p => p)
                .First(g => g.Count() > 1)
                .Key;
            throw new DuplicatePlatformTypeException(duplicate);
        }

        Platforms = distinct;
    }
}
