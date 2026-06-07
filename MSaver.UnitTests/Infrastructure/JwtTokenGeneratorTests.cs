using System.IdentityModel.Tokens.Jwt;

using Microsoft.Extensions.Configuration;

using MSaver.Infrastructure.Auth;

namespace MSaver.UnitTests.Infrastructure;

public sealed class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateAccessToken_ShouldThrowConfigurationError_WhenSigningKeyIsTooShort()
    {
        var configuration = CreateConfiguration(("JwtSettings:Key", "short-key"));
        var generator = new JwtTokenGenerator(configuration);

        var action = () => generator.GenerateAccessToken(
            Guid.NewGuid(),
            "rostik",
            "rostik@example.com",
            "client-1");

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*JwtSettings:Key*32*");
    }

    [Fact]
    public void GenerateAccessToken_ShouldCreateTokenWithClientIdAndTokenType_WhenSettingsAreValid()
    {
        var configuration = CreateConfiguration(
            ("JwtSettings:Key", "0123456789abcdef0123456789abcdef"),
            ("JwtSettings:Issuer", "MSaver"),
            ("JwtSettings:Audience", "MSaverFrontend"),
            ("JwtSettings:AccessTokenMinutes", "15"));
        var generator = new JwtTokenGenerator(configuration);
        var userId = Guid.NewGuid();

        var token = generator.GenerateAccessToken(
            userId,
            "rostik",
            "rostik@example.com",
            "client-1");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Should().Contain(x => x.Type == JwtRegisteredClaimNames.Sub && x.Value == userId.ToString());
        jwt.Claims.Should().Contain(x => x.Type == JwtRegisteredClaimNames.Typ && x.Value == "access");
        jwt.Claims.Should().Contain(x => x.Type == "client_id" && x.Value == "client-1");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldIncludeUniqueJwtId()
    {
        var configuration = CreateConfiguration(
            ("JwtSettings:Key", "0123456789abcdef0123456789abcdef"),
            ("JwtSettings:Issuer", "MSaver"),
            ("JwtSettings:Audience", "MSaverFrontend"),
            ("JwtSettings:RefreshTokenDays", "30"));
        var generator = new JwtTokenGenerator(configuration);
        var userId = Guid.NewGuid();

        var firstToken = generator.GenerateRefreshToken(
            userId,
            "rostik",
            "rostik@example.com",
            "client-1").token;
        var secondToken = generator.GenerateRefreshToken(
            userId,
            "rostik",
            "rostik@example.com",
            "client-1").token;

        var handler = new JwtSecurityTokenHandler();
        var firstJwt = handler.ReadJwtToken(firstToken);
        var secondJwt = handler.ReadJwtToken(secondToken);

        firstJwt.Claims.Should().Contain(x => x.Type == JwtRegisteredClaimNames.Jti);
        secondJwt.Claims.Should().Contain(x => x.Type == JwtRegisteredClaimNames.Jti);
        firstJwt.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value
            .Should()
            .NotBe(secondJwt.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value);
    }

    private static IConfiguration CreateConfiguration(params (string Key, string Value)[] values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(x => x.Key, x => (string?)x.Value))
            .Build();
    }
}
