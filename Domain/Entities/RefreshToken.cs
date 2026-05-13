namespace MSaver.Domain.Entities;

public sealed class RefreshToken : Entity
{
    private RefreshToken() { }

    public Guid UserId { get; private set; }

    public string Token { get; private set; } = null!;

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
    {
        if (userId == Guid.Empty)
            throw new DomainException(UserDomainErrors.UserIdRequired);

        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException(AuthDomainErrors.RefreshTokenInvalid);

        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Revoke()
    {
        ExpiresAt = DateTime.UtcNow;
    }
}