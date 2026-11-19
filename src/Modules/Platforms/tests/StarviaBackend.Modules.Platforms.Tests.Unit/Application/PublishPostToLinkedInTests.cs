using FluentAssertions;
using StarviaBackend.Modules.Platforms.Application.LinkedIn.Commands.PublishPostToLinkedIn;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;
using StarviaBackend.Modules.Platforms.Tests.Unit.Fakes;
using StarviaBackend.Modules.Posts.Integrations;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Platforms.Tests.Unit.Application;

public sealed class PublishPostToLinkedInTests
{
    private readonly InMemoryPlatformRepository _platforms = new();
    private readonly InMemoryUserPlatformRepository _userPlatforms = new();
    private readonly FakeLinkedInClient _linkedIn = new();
    private readonly FakePostsClient _posts = new();
    private readonly FakeMediaClient _media = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    private PublishPostToLinkedInHandler CreateHandler() =>
        new(
            _userPlatforms,
            _platforms,
            TestAccess.For(_ownerId),
            _posts,
            _media,
            _linkedIn);

    [Fact]
    public async Task HandleAsync_Publishes_Text_And_Records_Publication()
    {
        var userPlatformId = SeedLinkedInConnection();
        var postId = _posts.Seed("Hello LinkedIn");

        var result = await CreateHandler().HandleAsync(
            new PublishPostToLinkedInCommand(new PublishPostToLinkedInRequest(postId, userPlatformId), _ownerId));

        result.ExternalPostId.Should().Be("urn:li:share:text");
        _linkedIn.TextPosts.Should().ContainSingle(p => p.Content == "Hello LinkedIn");
        _posts.CreatedPublications.Should().ContainSingle(p =>
            p.PostId == postId
            && p.UserPlatformId == userPlatformId
            && p.ExternalPostId == "urn:li:share:text");
    }

    [Fact]
    public async Task HandleAsync_Publishes_Images_When_Post_Has_Attachments()
    {
        var userPlatformId = SeedLinkedInConnection();
        var fileId = Guid.NewGuid();
        var postId = _posts.Seed("With image", [new PostAttachmentRef(fileId, 0)]);

        var result = await CreateHandler().HandleAsync(
            new PublishPostToLinkedInCommand(new PublishPostToLinkedInRequest(postId, userPlatformId), _ownerId));

        result.ExternalPostId.Should().Be("urn:li:share:image");
        _linkedIn.ImagePosts.Should().ContainSingle(p => p.Content == "With image" && p.Images.Count == 1);
        _posts.CreatedPublications.Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_Publishes_Article_When_ArticleUrl_Provided()
    {
        var userPlatformId = SeedLinkedInConnection();
        var postId = _posts.Seed(
            "Learning more about LinkedIn by reading the LinkedIn Blog!",
            title: "Official LinkedIn Blog");

        var result = await CreateHandler().HandleAsync(
            new PublishPostToLinkedInCommand(
                new PublishPostToLinkedInRequest(
                    postId,
                    userPlatformId,
                    ArticleUrl: "https://blog.linkedin.com/",
                    ArticleDescription: "Official LinkedIn Blog - Your source for insights."),
                _ownerId));

        result.ExternalPostId.Should().Be("urn:li:share:article");
        var article = _linkedIn.ArticlePosts.Should().ContainSingle().Subject;
        article.Content.Should().Be("Learning more about LinkedIn by reading the LinkedIn Blog!");
        article.Url.Should().Be("https://blog.linkedin.com/");
        article.Title.Should().Be("Official LinkedIn Blog");
        article.Description.Should().Be("Official LinkedIn Blog - Your source for insights.");
        _posts.CreatedPublications.Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Body_Is_Empty()
    {
        var userPlatformId = SeedLinkedInConnection();
        var postId = _posts.Seed("   ");

        var act = () => CreateHandler().HandleAsync(
            new PublishPostToLinkedInCommand(new PublishPostToLinkedInRequest(postId, userPlatformId), _ownerId));

        await act.Should().ThrowAsync<PostBodyRequiredException>();
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Account_Is_Not_LinkedIn()
    {
        var facebook = _platforms.Seed(PlatformType.Facebook);
        var userPlatform = UserPlatform.Create(_ownerId, facebook.Id, "fb-user", "token", "ext", null, null);
        await _userPlatforms.AddAsync(userPlatform);
        var postId = _posts.Seed();

        var act = () => CreateHandler().HandleAsync(
            new PublishPostToLinkedInCommand(new PublishPostToLinkedInRequest(postId, userPlatform.Id), _ownerId));

        await act.Should().ThrowAsync<UserPlatformPlatformMismatchException>();
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Account_Is_Not_Connected()
    {
        var linkedIn = _platforms.Seed(PlatformType.LinkedIn);
        var userPlatform = UserPlatform.Create(_ownerId, linkedIn.Id, "li-user", accountComment: null);
        await _userPlatforms.AddAsync(userPlatform);
        var postId = _posts.Seed();

        var act = () => CreateHandler().HandleAsync(
            new PublishPostToLinkedInCommand(new PublishPostToLinkedInRequest(postId, userPlatform.Id), _ownerId));

        await act.Should().ThrowAsync<UserPlatformNotConnectedException>();
    }

    private Guid SeedLinkedInConnection()
    {
        var linkedIn = _platforms.Seed(PlatformType.LinkedIn);
        var userPlatform = UserPlatform.Create(
            _ownerId,
            linkedIn.Id,
            "Jane Doe",
            "li-token",
            "sub-123",
            null,
            null);
        _userPlatforms.UserPlatforms.Add(userPlatform);
        return userPlatform.Id;
    }
}
