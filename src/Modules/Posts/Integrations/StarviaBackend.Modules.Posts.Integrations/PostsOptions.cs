namespace StarviaBackend.Modules.Posts.Integrations;

public sealed class PostsOptions
{
    public const string SectionName = "integrations:posts";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
