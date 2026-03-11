using System.Security.Claims;
using server.Domain.Common;
using server.Domain.Errors;

namespace server.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("uid")?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            throw new DomainException(UserDomainErrors.IdNotFound);
        }

        return Guid.Parse(userIdClaim);
    }
}
