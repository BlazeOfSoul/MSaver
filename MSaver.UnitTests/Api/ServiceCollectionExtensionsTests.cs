using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MSaver.Api.Configuration;

namespace MSaver.UnitTests.Api;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApiServices_ShouldThrow_WhenDefaultConnectionStringIsMissing()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(
            ("ConnectionStrings:DefaultConnection", ""),
            ("JwtSettings:Key", "0123456789abcdef0123456789abcdef"),
            ("JwtSettings:Issuer", "MSaver"),
            ("JwtSettings:Audience", "MSaverFrontend"),
            ("Cors:FrontendOrigin", "http://localhost:4200"),
            ("ExchangeRateApi:BaseUrl", "https://api.example.test/"),
            ("ExchangeRateApi:ApiKey", "test-key"));

        var action = () => services.AddApiServices(configuration);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*ConnectionStrings:DefaultConnection*");
    }

    [Fact]
    public void AddApiServices_ShouldThrow_WhenCorsOriginIsWildcard()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(
            ("ConnectionStrings:DefaultConnection", "Host=localhost;Database=MSaverDb;Username=postgres"),
            ("JwtSettings:Key", "0123456789abcdef0123456789abcdef"),
            ("JwtSettings:Issuer", "MSaver"),
            ("JwtSettings:Audience", "MSaverFrontend"),
            ("Cors:FrontendOrigin", "*"),
            ("ExchangeRateApi:BaseUrl", "https://api.example.test/"),
            ("ExchangeRateApi:ApiKey", "test-key"));

        var action = () => services.AddApiServices(configuration);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Cors*wildcard*");
    }

    [Fact]
    public void AddApiServices_ShouldThrow_WhenJwtSigningKeyIsTooShort()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(
            ("ConnectionStrings:DefaultConnection", "Host=localhost;Database=MSaverDb;Username=postgres"),
            ("JwtSettings:Key", "short-key"),
            ("JwtSettings:Issuer", "MSaver"),
            ("JwtSettings:Audience", "MSaverFrontend"),
            ("Cors:FrontendOrigin", "http://localhost:4200"),
            ("ExchangeRateApi:BaseUrl", "https://api.example.test/"),
            ("ExchangeRateApi:ApiKey", "test-key"));

        var action = () => services.AddApiServices(configuration);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*JwtSettings:Key*32*");
    }

    private static IConfiguration CreateConfiguration(params (string Key, string Value)[] values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(x => x.Key, x => (string?)x.Value))
            .Build();
    }
}
