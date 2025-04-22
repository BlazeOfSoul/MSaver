using System.Security.Claims;

namespace server.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("uid")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new Exception("User ID не найден в токене.");
        }

        return Guid.Parse(userIdClaim);
    }
}

