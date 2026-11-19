using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using StarviaBackend.Modules.Platforms.Integrations.LinkedIn.External;

namespace StarviaBackend.Modules.Platforms.Integrations.LinkedIn;

/// <summary>
/// Typed HttpClient for LinkedIn OAuth + Share on LinkedIn (ugcPosts). Owns the wire format and
/// translates transport failures into <see cref="LinkedInIntegrationException"/>.
/// </summary>
internal sealed class LinkedInClient(HttpClient httpClient, IOptions<LinkedInOptions> options) : ILinkedInClient
{
    private const string Scopes = "openid profile email w_member_social";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly LinkedInOptions _options = options.Value;

    public string SuccessRedirectUrl => _options.SuccessRedirectUrl;

    public string BuildAuthorizationUrl(Guid userId, Guid platformId)
    {
        EnsureConfigured();

        var state = LinkedInOAuthState.Create(userId, platformId);
        var baseUrl = EnsureTrailingSlash(_options.BaseUrl);

        return $"{baseUrl}authorization"
               + $"?response_type=code"
               + $"&client_id={Uri.EscapeDataString(_options.ClientId)}"
               + $"&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}"
               + $"&scope={Uri.EscapeDataString(Scopes)}"
               + $"&state={Uri.EscapeDataString(state)}";
    }

    public async Task<string> ExchangeCodeForAccessTokenAsync(string code, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new LinkedInIntegrationException("LinkedIn authorization code is missing.");
        }

        var values = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _options.RedirectUri,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
        };

        using var response = await SendAsync(
            () => httpClient.PostAsync(
                $"{EnsureTrailingSlash(_options.BaseUrl)}accessToken",
                new FormUrlEncodedContent(values),
                cancellationToken),
            cancellationToken);

        var payload = await ReadJsonAsync<LinkedInTokenResponse>(response, "token exchange", cancellationToken);
        if (string.IsNullOrWhiteSpace(payload.AccessToken))
        {
            throw new LinkedInIntegrationException("LinkedIn token exchange returned an empty access token.");
        }

        return payload.AccessToken;
    }

    public async Task<LinkedInAccountInfo> GetAccountInfoAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        EnsureAccessToken(accessToken);

        using var request = CreateAuthorizedRequest(HttpMethod.Get, $"{EnsureTrailingSlash(_options.ApiUrl)}userinfo", accessToken);
        using var response = await SendAsync(() => httpClient.SendAsync(request, cancellationToken), cancellationToken);

        var payload = await ReadJsonAsync<LinkedInUserInfoResponse>(response, "/userinfo", cancellationToken);
        if (string.IsNullOrWhiteSpace(payload.Sub) || string.IsNullOrWhiteSpace(payload.Name))
        {
            throw new LinkedInIntegrationException("LinkedIn /userinfo returned incomplete profile data.");
        }

        return new LinkedInAccountInfo(payload.Sub, payload.Name, payload.Picture);
    }

    public async Task<string> PublishTextPostAsync(LinkedInTextPost post, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(post);
        EnsureAccessToken(post.AccessToken);
        EnsureExternalAccountId(post.ExternalAccountId);

        if (string.IsNullOrWhiteSpace(post.Content))
        {
            throw new LinkedInIntegrationException("LinkedIn post content cannot be empty.");
        }

        var body = BuildUgcBody(post.ExternalAccountId, post.Content, shareMediaCategory: "NONE", media: null);
        return await PostUgcAsync(post.AccessToken, body, cancellationToken);
    }

    public async Task<string> PublishArticlePostAsync(LinkedInArticlePost post, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(post);
        EnsureAccessToken(post.AccessToken);
        EnsureExternalAccountId(post.ExternalAccountId);

        if (string.IsNullOrWhiteSpace(post.Content))
        {
            throw new LinkedInIntegrationException("LinkedIn post content cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(post.Url)
            || !Uri.TryCreate(post.Url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new LinkedInIntegrationException("LinkedIn article post requires an absolute http(s) URL.");
        }

        var mediaItem = new Dictionary<string, object?>
        {
            ["status"] = "READY",
            ["originalUrl"] = post.Url,
        };

        if (!string.IsNullOrWhiteSpace(post.Title))
        {
            mediaItem["title"] = new { text = post.Title };
        }

        if (!string.IsNullOrWhiteSpace(post.Description))
        {
            mediaItem["description"] = new { text = post.Description };
        }

        var body = BuildUgcBody(
            post.ExternalAccountId,
            post.Content,
            shareMediaCategory: "ARTICLE",
            media: [mediaItem]);

        return await PostUgcAsync(post.AccessToken, body, cancellationToken);
    }

    public async Task<string> PublishImagePostAsync(LinkedInImagePost post, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(post);
        EnsureAccessToken(post.AccessToken);
        EnsureExternalAccountId(post.ExternalAccountId);

        if (string.IsNullOrWhiteSpace(post.Content))
        {
            throw new LinkedInIntegrationException("LinkedIn post content cannot be empty.");
        }

        if (post.Images is null || post.Images.Count == 0)
        {
            throw new LinkedInIntegrationException("LinkedIn image post requires at least one image.");
        }

        var authorUrn = PersonUrn(post.ExternalAccountId);
        var media = new List<object>(post.Images.Count);

        foreach (var image in post.Images)
        {
            var (asset, uploadUrl) = await RegisterUploadAsync(post.AccessToken, authorUrn, cancellationToken);
            await UploadMediaAsync(post.AccessToken, uploadUrl, image, cancellationToken);
            media.Add(new { status = "READY", media = asset });
        }

        var body = BuildUgcBody(post.ExternalAccountId, post.Content, shareMediaCategory: "IMAGE", media);
        return await PostUgcAsync(post.AccessToken, body, cancellationToken);
    }

    private async Task<(string Asset, string UploadUrl)> RegisterUploadAsync(
        string accessToken,
        string ownerUrn,
        CancellationToken cancellationToken)
    {
        var body = new
        {
            registerUploadRequest = new
            {
                recipes = new[] { "urn:li:digitalmediaRecipe:feedshare-image" },
                owner = ownerUrn,
                serviceRelationships = new[]
                {
                    new
                    {
                        relationshipType = "OWNER",
                        identifier = "urn:li:userGeneratedContent",
                    },
                },
            },
        };

        using var request = CreateAuthorizedRequest(
            HttpMethod.Post,
            $"{EnsureTrailingSlash(_options.ApiUrl)}assets?action=registerUpload",
            accessToken);
        request.Content = JsonContent.Create(body);

        using var response = await SendAsync(() => httpClient.SendAsync(request, cancellationToken), cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, "register upload", json);

        using var document = JsonDocument.Parse(json);
        var value = document.RootElement.GetProperty("value");
        var asset = value.GetProperty("asset").GetString()
                    ?? throw new LinkedInIntegrationException("LinkedIn register upload returned no asset.");
        var uploadUrl = value
            .GetProperty("uploadMechanism")
            .GetProperty("com.linkedin.digitalmedia.uploading.MediaUploadHttpRequest")
            .GetProperty("uploadUrl")
            .GetString()
            ?? throw new LinkedInIntegrationException("LinkedIn register upload returned no upload URL.");

        return (asset, uploadUrl);
    }

    private async Task UploadMediaAsync(
        string accessToken,
        string uploadUrl,
        LinkedInImageSource image,
        CancellationToken cancellationToken)
    {
        if (image.Content.CanSeek)
        {
            image.Content.Position = 0;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StreamContent(image.Content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(image.ContentType) ? "application/octet-stream" : image.ContentType);

        using var response = await SendAsync(() => httpClient.SendAsync(request, cancellationToken), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new LinkedInIntegrationException($"LinkedIn media upload failed: {(int)response.StatusCode} - {error}");
        }
    }

    private async Task<string> PostUgcAsync(string accessToken, object body, CancellationToken cancellationToken)
    {
        var raw = JsonSerializer.Serialize(body)
            .Replace("com_linkedin_ugc_ShareContent", "com.linkedin.ugc.ShareContent", StringComparison.Ordinal)
            .Replace("com_linkedin_ugc_MemberNetworkVisibility", "com.linkedin.ugc.MemberNetworkVisibility", StringComparison.Ordinal);

        using var request = CreateAuthorizedRequest(
            HttpMethod.Post,
            $"{EnsureTrailingSlash(_options.ApiUrl)}ugcPosts",
            accessToken);
        request.Content = new StringContent(raw, Encoding.UTF8, "application/json");

        using var response = await SendAsync(() => httpClient.SendAsync(request, cancellationToken), cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, "ugcPosts", json);

        // LinkedIn may return a JSON string URN or an object with "id".
        if (json.StartsWith('"'))
        {
            return JsonSerializer.Deserialize<string>(json)
                   ?? throw new LinkedInIntegrationException("LinkedIn ugcPosts returned an empty id.");
        }

        var payload = JsonSerializer.Deserialize<LinkedInUgcPostResponse>(json, JsonOptions)
                      ?? throw new LinkedInIntegrationException("LinkedIn ugcPosts returned an empty body.");

        if (string.IsNullOrWhiteSpace(payload.Id))
        {
            // Prefer Rest.li header when the body has no id.
            if (response.Headers.TryGetValues("X-RestLi-Id", out var ids))
            {
                var headerId = ids.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(headerId))
                {
                    return headerId;
                }
            }

            throw new LinkedInIntegrationException("LinkedIn ugcPosts returned no post id.");
        }

        return payload.Id;
    }

    private static object BuildUgcBody(string externalAccountId, string text, string shareMediaCategory, List<object>? media)
    {
        object shareContent = media is null
            ? new
            {
                shareCommentary = new { text },
                shareMediaCategory,
            }
            : new
            {
                shareCommentary = new { text },
                shareMediaCategory,
                media,
            };

        return new
        {
            author = PersonUrn(externalAccountId),
            lifecycleState = "PUBLISHED",
            specificContent = new
            {
                com_linkedin_ugc_ShareContent = shareContent,
            },
            visibility = new
            {
                com_linkedin_ugc_MemberNetworkVisibility = "PUBLIC",
            },
        };
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.TryAddWithoutValidation("X-Restli-Protocol-Version", "2.0.0");
        return request;
    }

    private async Task<HttpResponseMessage> SendAsync(
        Func<Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        try
        {
            return await send();
        }
        catch (Exception ex) when (ex is HttpRequestException
                                   || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            throw new LinkedInIntegrationException($"LinkedIn API unreachable: {ex.Message}");
        }
    }

    private static async Task<T> ReadJsonAsync<T>(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, operation, json);

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions)
                   ?? throw new LinkedInIntegrationException($"LinkedIn {operation} returned an empty body.");
        }
        catch (JsonException)
        {
            throw new LinkedInIntegrationException($"LinkedIn {operation} returned invalid JSON.");
        }
    }

    private static void EnsureSuccess(HttpResponseMessage response, string operation, string body)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new LinkedInIntegrationException(
                $"LinkedIn {operation} failed: {(int)response.StatusCode} - {Truncate(body)}");
        }
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId)
            || string.IsNullOrWhiteSpace(_options.ClientSecret)
            || string.IsNullOrWhiteSpace(_options.RedirectUri))
        {
            throw new LinkedInIntegrationException("LinkedIn OAuth is not configured (ClientId / ClientSecret / RedirectUri).");
        }
    }

    private static void EnsureAccessToken(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new LinkedInIntegrationException("LinkedIn access token is missing.");
        }
    }

    private static void EnsureExternalAccountId(string externalAccountId)
    {
        if (string.IsNullOrWhiteSpace(externalAccountId))
        {
            throw new LinkedInIntegrationException("LinkedIn external account id is missing.");
        }
    }

    private static string PersonUrn(string externalAccountId) => $"urn:li:person:{externalAccountId}";

    private static string EnsureTrailingSlash(string url) => url.EndsWith('/') ? url : $"{url}/";

    private static string Truncate(string value) =>
        value.Length <= 500 ? value : value[..500] + "…";
}
