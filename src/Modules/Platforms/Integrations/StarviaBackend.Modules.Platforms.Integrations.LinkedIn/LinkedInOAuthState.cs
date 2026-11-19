namespace StarviaBackend.Modules.Platforms.Integrations.LinkedIn;

/// <summary>
/// Encodes user + platform into the OAuth <c>state</c> so the anonymous callback can finish the link.
/// Format: <c>{userId}|{platformId}|{nonce}</c>.
/// </summary>
public static class LinkedInOAuthState
{
    public static string Create(Guid userId, Guid platformId) =>
        $"{userId:D}|{platformId:D}|{Guid.NewGuid():N}";

    public static bool TryParse(string? state, out Guid userId, out Guid platformId)
    {
        userId = default;
        platformId = default;

        if (string.IsNullOrWhiteSpace(state))
        {
            return false;
        }

        var parts = state.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            return false;
        }

        return Guid.TryParse(parts[0], out userId) && Guid.TryParse(parts[1], out platformId);
    }
}
