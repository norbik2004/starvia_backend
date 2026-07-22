namespace StarviaBackend.Shared.Infrastructure.Auth;

public sealed class AuthOptions
{
    public const string SectionName = "auth";

    public string Issuer { get; set; } = "Starvia";
    public string Audience { get; set; } = "Starvia";

    /// <summary>Symmetric signing key. Override with a strong secret in every real environment.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}
