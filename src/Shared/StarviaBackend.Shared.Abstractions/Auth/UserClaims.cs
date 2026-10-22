namespace StarviaBackend.Shared.Abstractions.Auth;

/// <summary>Custom claim type names used in issued tokens.</summary>
public static class UserClaims
{
    public const string UserId = "sub";
    public const string Email = "email";
    public const string Role = "role";
}
