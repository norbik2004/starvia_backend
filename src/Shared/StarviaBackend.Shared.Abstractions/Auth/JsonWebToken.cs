namespace StarviaBackend.Shared.Abstractions.Auth;

/// <summary>An issued access token plus the claims a client typically needs up front.</summary>
public sealed record JsonWebToken(
    string AccessToken,
    long ExpiresAt,
    string UserId,
    string Email,
    IReadOnlyList<string> Roles);
