namespace StarviaBackend.Modules.Platforms.Integrations.LinkedIn;

public sealed class LinkedInOptions
{
    public const string SectionName = "integrations:linkedin";

    /// <summary>OAuth base, e.g. https://www.linkedin.com/oauth/v2/</summary>
    public string BaseUrl { get; set; } = "https://www.linkedin.com/oauth/v2/";

    /// <summary>REST API base, e.g. https://api.linkedin.com/v2/</summary>
    public string ApiUrl { get; set; } = "https://api.linkedin.com/v2/";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Must match a redirect URL registered in the LinkedIn Developer Portal.</summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>Frontend URL the browser is sent to after a successful account link.</summary>
    public string SuccessRedirectUrl { get; set; } = "http://localhost:4200/dashboard/social-accounts";

    public int TimeoutSeconds { get; set; } = 30;
}
