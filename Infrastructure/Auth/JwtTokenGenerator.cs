using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace MSaver.Infrastructure.Auth;

public sealed class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration = configuration;

    public string GenerateAccessToken(Guid userId, string username, string email, string clientId)
    {
        var expires = DateTime.UtcNow.AddMinutes(
            int.Parse(_configuration["JwtSettings:AccessTokenMinutes"] ?? "60"));

        return GenerateToken(userId, username, email, clientId, expires, "access");
    }

    public (string token, DateTime expiresAt) GenerateRefreshToken(
        Guid userId,
        string username,
        string email,
        string clientId)
    {
        var expires = DateTime.UtcNow.AddDays(
            int.Parse(_configuration["JwtSettings:RefreshTokenDays"] ?? "30"));

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
            new("client_id", clientId)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}