namespace MSaver.Domain.Entities;

public sealed class RefreshToken : Entity
{
    private RefreshToken() { }

    public Guid UserId { get; private set; }

    public string ClientId { get; private set; } = null!;

    public string Token { get; private set; } = null!;

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public static RefreshToken Create(Guid userId, string clientId, string token, DateTime expiresAt)
    {
        if (userId == Guid.Empty)
            throw new DomainException(UserDomainErrors.UserIdRequired);

        if (string.IsNullOrWhiteSpace(clientId))
            throw new DomainException(AuthDomainErrors.RefreshTokenInvalid);

        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException(AuthDomainErrors.RefreshTokenInvalid);

        return new RefreshToken
        {
            UserId = userId,
            ClientId = clientId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Replace(string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException(AuthDomainErrors.RefreshTokenInvalid);

        Token = token;
        ExpiresAt = expiresAt;
    }

    public void Revoke()
    {
        ExpiresAt = DateTime.UtcNow;
    }
}