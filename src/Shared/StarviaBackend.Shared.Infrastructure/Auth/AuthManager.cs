using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Shared.Infrastructure.Auth;

internal sealed class AuthManager(IOptions<AuthOptions> options, IClock clock) : IAuthManager
{
    private readonly AuthOptions _options = options.Value;

    public JsonWebToken CreateToken(string userId, string email, IEnumerable<string> roles)
    {
        var now = clock.UtcNow;
        var expires = now.AddMinutes(_options.ExpiryMinutes);
        var roleList = roles.ToList();

        var claims = new List<Claim>
        {
            new(UserClaims.UserId, userId),
            new(UserClaims.Email, email),
        };
        claims.AddRange(roleList.Select(role => new Claim(UserClaims.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new JsonWebToken(accessToken, new DateTimeOffset(expires).ToUnixTimeSeconds(), userId, email, roleList);
    }
}
