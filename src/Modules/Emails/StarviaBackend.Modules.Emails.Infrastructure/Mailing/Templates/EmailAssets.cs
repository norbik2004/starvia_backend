namespace StarviaBackend.Modules.Emails.Infrastructure.Mailing.Templates;

internal static class EmailAssets
{
    private const string StarviaLogoFileName = "starvia-logo.png";

    private static readonly Lazy<string> StarviaLogoDataUriLazy = new(() =>
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Mailing",
            "Templates",
            "Shared",
            StarviaLogoFileName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Email logo asset not found: {path}");
        }

        var bytes = File.ReadAllBytes(path);
        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    });

    public static string StarviaLogoDataUri => StarviaLogoDataUriLazy.Value;
}
