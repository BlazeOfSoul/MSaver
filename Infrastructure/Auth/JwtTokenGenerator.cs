using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

using MSaver.Infrastructure.Configuration;

namespace MSaver.Infrastructure.Auth;

public sealed class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration = configuration;

    public string GenerateAccessToken(Guid userId, string username, string email, string clientId)
    {
        var expires = DateTime.UtcNow.AddMinutes(
            JwtSettings.GetAccessTokenMinutes(_configuration));

        return GenerateToken(userId, username, email, clientId, expires, "access");
    }

    public (string token, DateTime expiresAt) GenerateRefreshToken(
        Guid userId,
        string username,
        string email,
        string clientId)
    {
        var expires = DateTime.UtcNow.AddDays(
            JwtSettings.GetRefreshTokenDays(_configuration));

        var token = GenerateToken(userId, username, email, clientId, expires, "refresh");

        return (token, expires);
    }

    private string GenerateToken(
        Guid userId,
        string username,
        string email,
        string clientId,
        DateTime expires,
        string tokenType)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Typ, tokenType),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("client_id", clientId)
        };

        var creds = new SigningCredentials(
            JwtSettings.CreateSigningKey(_configuration),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: JwtSettings.GetIssuer(_configuration),
            audience: JwtSettings.GetAudience(_configuration),
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
