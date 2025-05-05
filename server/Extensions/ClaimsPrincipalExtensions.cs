using System.Security.Claims;

using server.Models.Constants;

namespace server.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("uid")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new Exception(ErrorMessages.User.IdNotFound);
        }

        return Guid.Parse(userIdClaim);
    }
}

