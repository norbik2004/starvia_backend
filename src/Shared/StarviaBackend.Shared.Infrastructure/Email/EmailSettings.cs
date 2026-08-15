namespace StarviaBackend.Shared.Infrastructure.Email;

public sealed class EmailSettings
{
    public const string SectionName = "email";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public bool UseSsl { get; set; }
    public bool DefaultCredentials { get; set; } = true;
    public string EmailId { get; set; } = "noreply@starvia.local";
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = "Starvia";
}
