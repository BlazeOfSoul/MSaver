using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MSaver.Api.Auth;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid UserId
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal is null || principal.Identity?.IsAuthenticated != true)
                throw new DomainException(UserDomainErrors.IdNotFound);

            var userIdClaim =
                principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
                throw new DomainException(UserDomainErrors.IdNotFound);

            if (!Guid.TryParse(userIdClaim, out var userId))
                throw new DomainException(UserDomainErrors.IdNotFound);

            return userId;
        }
    }

    public string? Username =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

    public string? Email =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

    public string ClientId
    {
        get
        {
            var clientId = _httpContextAccessor.HttpContext?.User.FindFirst("client_id")?.Value;

            if (string.IsNullOrWhiteSpace(clientId))
                throw new DomainException(UserDomainErrors.IdNotFound);

            return clientId;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
