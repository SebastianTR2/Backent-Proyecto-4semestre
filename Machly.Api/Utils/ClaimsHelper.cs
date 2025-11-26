using System.Security.Claims;

namespace Machly.Api.Utils
{
    public static class ClaimsHelper
    {
        public static string? GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirst("id")?.Value;
        }

        public static string? GetUserEmail(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }

        public static string? GetUserRole(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}

