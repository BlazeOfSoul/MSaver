using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace MSaver.Infrastructure.Configuration;

public static class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public const int MinimumSigningKeyBytes = 32;

    public static string GetIssuer(IConfiguration configuration) =>
        GetRequiredValue(configuration, "Issuer");

    public static string GetAudience(IConfiguration configuration) =>
        GetRequiredValue(configuration, "Audience");

    public static int GetAccessTokenMinutes(IConfiguration configuration) =>
        GetPositiveInt(configuration, "AccessTokenMinutes", 60);

    public static int GetRefreshTokenDays(IConfiguration configuration) =>
        GetPositiveInt(configuration, "RefreshTokenDays", 30);

    public static SymmetricSecurityKey CreateSigningKey(IConfiguration configuration)
    {
        var key = GetRequiredValue(configuration, "Key");
        var keyBytes = Encoding.UTF8.GetBytes(key);

        if (keyBytes.Length < MinimumSigningKeyBytes)
        {
            throw new InvalidOperationException(
                $"JwtSettings:Key must contain at least {MinimumSigningKeyBytes} UTF-8 bytes.");
        }

        return new SymmetricSecurityKey(keyBytes);
    }

    private static string GetRequiredValue(IConfiguration configuration, string name)
    {
        var value = configuration[$"{SectionName}:{name}"];

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{SectionName}:{name} configuration is missing.");

        return value;
    }

    private static int GetPositiveInt(
        IConfiguration configuration,
        string name,
        int defaultValue)
    {
        var rawValue = configuration[$"{SectionName}:{name}"];

        if (string.IsNullOrWhiteSpace(rawValue))
            return defaultValue;

        if (!int.TryParse(rawValue, out var value) || value <= 0)
            throw new InvalidOperationException($"{SectionName}:{name} must be a positive integer.");

        return value;
    }
}
