using System.Security.Claims;
using server.Application.Constants;

namespace server.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("uid")?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            throw new InvalidOperationException(ErrorMessages.User.IdNotFound);
        }

        return Guid.Parse(userIdClaim);
    }
}
