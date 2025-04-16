namespace MSaver.Api.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string username, string email);
}
