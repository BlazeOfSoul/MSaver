using System.Security.Cryptography;
using System.Text;

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
            Token = HashToken(token),
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static string HashToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException(AuthDomainErrors.RefreshTokenInvalid);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public void Replace(string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException(AuthDomainErrors.RefreshTokenInvalid);

        Token = HashToken(token);
        ExpiresAt = expiresAt;
    }

    public void Revoke()
    {
        ExpiresAt = DateTime.UtcNow;
    }
}
