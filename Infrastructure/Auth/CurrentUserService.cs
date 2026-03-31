using System.Security.Claims;

using Microsoft.AspNetCore.Http;

using MSaver.Application.Abstractions.Auth;
using MSaver.Domain.Common;
using MSaver.Domain.Errors;

namespace MSaver.Infrastructure.Auth;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal is null || principal.Identity?.IsAuthenticated != true)
                throw new DomainException(UserDomainErrors.IdNotFound);

            var userIdClaim =
                principal.FindFirst("uid")?.Value ??
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
                throw new DomainException(UserDomainErrors.IdNotFound);

            return Guid.Parse(userIdClaim);
        }
    }

    public string? Username =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

    public string? Email =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}