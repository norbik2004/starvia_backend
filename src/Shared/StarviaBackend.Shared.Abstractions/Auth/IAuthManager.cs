namespace StarviaBackend.Shared.Abstractions.Auth;

/// <summary>Creates signed access tokens. Implemented in Shared.Infrastructure.</summary>
public interface IAuthManager
{
    JsonWebToken CreateToken(string userId, string email, IEnumerable<string> roles);
}
