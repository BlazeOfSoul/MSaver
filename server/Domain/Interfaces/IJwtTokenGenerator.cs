namespace server.Domain.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string username, string email);
}
