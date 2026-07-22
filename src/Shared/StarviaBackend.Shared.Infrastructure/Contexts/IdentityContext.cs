using System.Security.Claims;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Shared.Infrastructure.Contexts;

internal sealed class IdentityContext : IIdentityContext
{
    public bool IsAuthenticated { get; }
    public Guid? UserId { get; }
    public string? Email { get; }
    public IReadOnlyList<string> Roles { get; }

    public IdentityContext(ClaimsPrincipal? principal)
    {
        IsAuthenticated = principal?.Identity?.IsAuthenticated ?? false;
        if (!IsAuthenticated || principal is null)
        {
            Roles = [];
            return;
        }

        var id = principal.FindFirstValue(UserClaims.UserId);
        UserId = Guid.TryParse(id, out var parsed) ? parsed : null;
        Email = principal.FindFirstValue(UserClaims.Email);
        Roles = principal.FindAll(UserClaims.Role).Select(c => c.Value).ToArray();
    }

    private IdentityContext()
    {
        Roles = [];
    }

    public static IdentityContext Empty => new();
}
