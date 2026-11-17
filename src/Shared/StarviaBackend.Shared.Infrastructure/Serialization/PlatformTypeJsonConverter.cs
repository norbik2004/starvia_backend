using System.Text.Json;
using System.Text.Json.Serialization;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Shared.Infrastructure.Serialization;

/// <summary>
/// Wire format for <see cref="PlatformType"/>: enum names (e.g. "Facebook"), while ints remain accepted.
/// Kept out of EF jsonb storage so the DB continues to store numeric enum values.
/// </summary>
public sealed class PlatformTypeJsonConverter : JsonConverter<PlatformType>
{
    public override PlatformType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var number))
        {
            if (Enum.IsDefined(typeof(PlatformType), number))
            {
                return (PlatformType)number;
            }

            throw new JsonException($"Invalid platform type value '{number}'.");
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var raw = reader.GetString();
            if (Enum.TryParse(raw, ignoreCase: true, out PlatformType platform) &&
                Enum.IsDefined(platform))
            {
                return platform;
            }

            throw new JsonException($"Invalid platform type value '{raw}'.");
        }

        throw new JsonException($"Unexpected token {reader.TokenType} for platform type.");
    }

    public override void Write(Utf8JsonWriter writer, PlatformType value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}
