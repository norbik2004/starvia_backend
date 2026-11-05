using System.Text.Json;

namespace StarviaBackend.Modules.Ai.Application.PostGeneration;

/// <summary>A post as written by the AI.</summary>
internal sealed record GeneratedPost(string Body);

/// <summary>
/// Reads the answer of the post generation agent. The agent is told to answer with a bare JSON object, but models
/// still wrap it in a code block or add a remark around it, so the object is cut out of the text before parsing.
/// Anything that is not an object with a non-empty <c>title</c> and <c>body</c> is rejected, because a post that
/// was only half understood must never reach the user.
/// </summary>
internal static class GeneratedPostParser
{
    public static GeneratedPost Parse(string content)
    {
        var json = ExtractJsonObject(content) ?? throw new InvalidGeneratedPostException();

        try
        {
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidGeneratedPostException();
            }

            var body = ReadString(document.RootElement, "body");

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new InvalidGeneratedPostException();
            }

            return new GeneratedPost(body.Trim());
        }
        catch (JsonException)
        {
            throw new InvalidGeneratedPostException();
        }
    }

    private static string? ExtractJsonObject(string content)
    {
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');

        return start >= 0 && end > start ? content[start..(end + 1)] : null;
    }

    private static string? ReadString(JsonElement root, string name)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
            }
        }

        return null;
    }
}
