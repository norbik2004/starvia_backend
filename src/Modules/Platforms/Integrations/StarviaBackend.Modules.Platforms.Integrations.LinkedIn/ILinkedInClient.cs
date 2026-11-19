namespace StarviaBackend.Modules.Platforms.Integrations.LinkedIn;

/// <summary>
/// Port the Platforms / Posts modules depend on. Callers speak in these terms and never see LinkedIn wire DTOs.
/// </summary>
public interface ILinkedInClient
{
    /// <summary>Builds the LinkedIn authorization URL (openid + profile + email + w_member_social).</summary>
    string BuildAuthorizationUrl(Guid userId, Guid platformId);

    /// <summary>Frontend URL to redirect the browser to after a successful OAuth callback.</summary>
    string SuccessRedirectUrl { get; }

    Task<string> ExchangeCodeForAccessTokenAsync(string code, CancellationToken cancellationToken = default);

    Task<LinkedInAccountInfo> GetAccountInfoAsync(string accessToken, CancellationToken cancellationToken = default);

    /// <summary>Publishes a text-only member post. Returns the LinkedIn post URN/id.</summary>
    Task<string> PublishTextPostAsync(LinkedInTextPost post, CancellationToken cancellationToken = default);

    /// <summary>Publishes an article/URL share with shareCommentary. Returns the LinkedIn post URN/id.</summary>
    Task<string> PublishArticlePostAsync(LinkedInArticlePost post, CancellationToken cancellationToken = default);

    /// <summary>Uploads images then publishes a member post. Returns the LinkedIn post URN/id.</summary>
    Task<string> PublishImagePostAsync(LinkedInImagePost post, CancellationToken cancellationToken = default);
}

public sealed record LinkedInAccountInfo(string Sub, string Name, string? ProfilePictureUrl);

public sealed record LinkedInTextPost(string AccessToken, string ExternalAccountId, string Content);

public sealed record LinkedInArticlePost(
    string AccessToken,
    string ExternalAccountId,
    string Content,
    string Url,
    string? Title = null,
    string? Description = null);

public sealed record LinkedInImagePost(
    string AccessToken,
    string ExternalAccountId,
    string Content,
    IReadOnlyList<LinkedInImageSource> Images);

/// <summary>Image bytes LinkedIn should receive, already downloaded by the caller.</summary>
public sealed record LinkedInImageSource(Stream Content, string ContentType);
