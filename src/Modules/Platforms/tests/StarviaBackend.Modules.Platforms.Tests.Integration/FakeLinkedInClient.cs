using StarviaBackend.Modules.Platforms.Integrations.LinkedIn;

namespace StarviaBackend.Modules.Platforms.Tests.Integration;

public sealed class FakeLinkedInClient : ILinkedInClient
{
    public string SuccessRedirectUrl => "http://localhost:4200/dashboard/social-accounts";
    public List<LinkedInTextPost> TextPosts { get; } = [];
    public List<LinkedInArticlePost> ArticlePosts { get; } = [];
    public List<LinkedInImagePost> ImagePosts { get; } = [];

    public void Reset()
    {
        TextPosts.Clear();
        ArticlePosts.Clear();
        ImagePosts.Clear();
    }

    public string BuildAuthorizationUrl(Guid userId, Guid platformId) =>
        $"https://linkedin.test/authorize?u={userId}&p={platformId}";

    public Task<string> ExchangeCodeForAccessTokenAsync(string code, CancellationToken cancellationToken = default)
        => Task.FromResult("token");

    public Task<LinkedInAccountInfo> GetAccountInfoAsync(string accessToken, CancellationToken cancellationToken = default)
        => Task.FromResult(new LinkedInAccountInfo("sub", "Name", null));

    public Task<string> PublishTextPostAsync(LinkedInTextPost post, CancellationToken cancellationToken = default)
    {
        TextPosts.Add(post);
        return Task.FromResult("urn:li:share:text-1");
    }

    public Task<string> PublishArticlePostAsync(LinkedInArticlePost post, CancellationToken cancellationToken = default)
    {
        ArticlePosts.Add(post);
        return Task.FromResult("urn:li:share:article-1");
    }

    public Task<string> PublishImagePostAsync(LinkedInImagePost post, CancellationToken cancellationToken = default)
    {
        ImagePosts.Add(post);
        return Task.FromResult("urn:li:share:image-1");
    }
}
