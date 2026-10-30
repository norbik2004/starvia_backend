namespace StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;

internal sealed class GoogleAiStudioOptions
{
    public const string SectionName = "integrations:googleaistudio";

    /// <summary>
    /// Secret key from Google AI Studio (aistudio.google.com/apikey). Never commit it: supply it through user
    /// secrets or the environment (<c>integrations__googleaistudio__ApiKey</c>).
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>The Gemini model that writes the texts, as named by the Gemini API (for example <c>gemini-3.5-flash-lite</c>).</summary>
    public string Model { get; set; } = "gemini-3.5-flash-lite";

    /// <summary>Generating text takes far longer than a typical API call, so the default is generous.</summary>
    public int TimeoutSeconds { get; set; } = 120;
}
