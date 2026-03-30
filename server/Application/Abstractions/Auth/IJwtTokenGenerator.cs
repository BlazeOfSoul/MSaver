namespace server.Application.Abstractions.Auth;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Guid userId, string username, string email);
    (string token, DateTime expiresAt) GenerateRefreshToken(Guid userId, string username, string email);
}
