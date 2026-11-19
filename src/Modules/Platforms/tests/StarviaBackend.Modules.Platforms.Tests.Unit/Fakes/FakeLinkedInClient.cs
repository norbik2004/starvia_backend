using StarviaBackend.Modules.Platforms.Integrations.LinkedIn;

namespace StarviaBackend.Modules.Platforms.Tests.Unit.Fakes;

internal sealed class FakeLinkedInClient : ILinkedInClient
{
    public string SuccessRedirectUrl { get; set; } = "http://localhost:4200/dashboard/social-accounts";
    public string AccessTokenToReturn { get; set; } = "access-token";
    public LinkedInAccountInfo AccountToReturn { get; set; } = new("sub-123", "Jane Doe", "https://example.com/p.png");
    public string? LastAuthorizationUrl { get; private set; }

    public List<LinkedInTextPost> TextPosts { get; } = [];
    public List<LinkedInArticlePost> ArticlePosts { get; } = [];
    public List<LinkedInImagePost> ImagePosts { get; } = [];

    public string BuildAuthorizationUrl(Guid userId, Guid platformId)
    {
        LastAuthorizationUrl = $"https://www.linkedin.com/oauth/v2/authorization?state={userId}|{platformId}|nonce";
        return LastAuthorizationUrl;
    }

    public Task<string> ExchangeCodeForAccessTokenAsync(string code, CancellationToken cancellationToken = default)
        => Task.FromResult(AccessTokenToReturn);

    public Task<LinkedInAccountInfo> GetAccountInfoAsync(string accessToken, CancellationToken cancellationToken = default)
        => Task.FromResult(AccountToReturn);

    public Task<string> PublishTextPostAsync(LinkedInTextPost post, CancellationToken cancellationToken = default)
    {
        TextPosts.Add(post);
        return Task.FromResult("urn:li:share:text");
    }

    public Task<string> PublishArticlePostAsync(LinkedInArticlePost post, CancellationToken cancellationToken = default)
    {
        ArticlePosts.Add(post);
        return Task.FromResult("urn:li:share:article");
    }

    public Task<string> PublishImagePostAsync(LinkedInImagePost post, CancellationToken cancellationToken = default)
    {
        ImagePosts.Add(post);
        return Task.FromResult("urn:li:share:image");
    }
}
