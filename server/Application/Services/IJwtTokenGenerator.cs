namespace server.Application.Abstractions.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string username, string email);
}
