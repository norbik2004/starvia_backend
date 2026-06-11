using System.Security.Claims;
using Core.Domain.Entities;
using Service.Exceptions;

namespace Web.Helpers
{
    public static class UserHelpers
    {
        public static string GetUserIdFromClaims(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedException("User is not logged in");
        }
    }
}
