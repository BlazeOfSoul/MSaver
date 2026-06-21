namespace MSaver.Application.Abstractions.Auth;

public interface ICurrentUserService
{
    Guid UserId { get; }

    string? Username { get; }

    string? Email { get; }

    string ClientId { get; }

    bool IsAuthenticated { get; }
}
