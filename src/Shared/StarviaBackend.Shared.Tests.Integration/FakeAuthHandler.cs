using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Shared.Tests.Integration;

/// <summary>
/// Replaces JWT auth in tests. Reads the identity from request headers so tests can act as any
/// user/role without minting real tokens. Mirrors how the real token encodes claims.
/// </summary>
public sealed class FakeAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "FakeAuth";
    public const string UserIdHeader = "X-Test-UserId";
    public const string EmailHeader = "X-Test-Email";
    public const string RolesHeader = "X-Test-Roles";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(UserIdHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = Request.Headers[UserIdHeader].ToString();
        var email = Request.Headers.TryGetValue(EmailHeader, out var e) ? e.ToString() : "test@example.com";

        var claims = new List<Claim>
        {
            new(UserClaims.UserId, userId),
            new(UserClaims.Email, email),
        };

        if (Request.Headers.TryGetValue(RolesHeader, out var roles))
        {
            claims.AddRange(roles.ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(role => new Claim(UserClaims.Role, role)));
        }

        var identity = new ClaimsIdentity(claims, SchemeName, UserClaims.UserId, UserClaims.Role);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
