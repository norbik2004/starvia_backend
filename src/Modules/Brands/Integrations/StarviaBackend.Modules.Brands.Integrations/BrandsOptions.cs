namespace StarviaBackend.Modules.Brands.Integrations;

public sealed class BrandsOptions
{
    public const string SectionName = "integrations:brands";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
