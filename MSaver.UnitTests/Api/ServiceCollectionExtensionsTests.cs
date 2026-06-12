using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using MSaver.Api.Configuration;
using MSaver.Api.Controllers;
using MSaver.Application.Abstractions.Services;
using MSaver.Infrastructure.Auth;

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

    [Fact]
    public void AddApiServices_ShouldThrow_WhenExchangeRateApiKeyIsMissing()
    {
        var services = new ServiceCollection();
        var settings = CreateValidSettings();
        settings["ExchangeRateApi:ApiKey"] = "";
        var configuration = CreateConfiguration(settings);

        var action = () => services.AddApiServices(configuration);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*ExchangeRateApi:ApiKey*");
    }

    [Fact]
    public void AddApiServices_ShouldThrow_WhenExchangeRateApiBaseUrlIsInvalid()
    {
        var services = new ServiceCollection();
        var settings = CreateValidSettings();
        settings["ExchangeRateApi:BaseUrl"] = "not-a-url";
        var configuration = CreateConfiguration(settings);

        var action = () => services.AddApiServices(configuration);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*ExchangeRateApi:BaseUrl*");
    }

    [Fact]
    public void AddApiServices_ShouldConfigureExchangeRateHttpClientTimeout()
    {
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();

        services.AddApiServices(configuration);

        using var provider = services.BuildServiceProvider();
        var client = provider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(nameof(IExchangeRateService));

        client.BaseAddress.Should().Be(new Uri("https://api.example.test/"));
        client.Timeout.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void AddApiServices_ShouldConfigureForwardedHeadersForReverseProxy()
    {
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();

        services.AddApiServices(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider
            .GetRequiredService<IOptions<ForwardedHeadersOptions>>()
            .Value;

        options.ForwardedHeaders.Should().HaveFlag(ForwardedHeaders.XForwardedFor);
        options.ForwardedHeaders.Should().HaveFlag(ForwardedHeaders.XForwardedProto);
        options.KnownIPNetworks.Should().NotBeEmpty();
        options.KnownProxies.Should().NotBeEmpty();
        options.ForwardLimit.Should().Be(1);
    }

    [Fact]
    public void AddApiServices_ShouldConfigureTrustedForwardedHeaderSourcesFromConfiguration()
    {
        var services = new ServiceCollection();
        var settings = CreateValidSettings();
        settings["ForwardedHeaders:KnownProxies:0"] = "10.10.0.5";
        settings["ForwardedHeaders:KnownNetworks:0"] = "10.20.0.0/16";
        var configuration = CreateConfiguration(settings);

        services.AddApiServices(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider
            .GetRequiredService<IOptions<ForwardedHeadersOptions>>()
            .Value;

        var expectedProxy = IPAddress.Parse("10.10.0.5");
        options.KnownProxies.Should().ContainSingle(proxy => proxy.Equals(expectedProxy));

        var network = options.KnownIPNetworks.Should().ContainSingle().Subject;
        network.ToString().Should().Be("10.20.0.0/16");
        network.PrefixLength.Should().Be(16);
    }

    [Fact]
    public void AddApiServices_ShouldRegisterLiveAndReadyHealthChecks()
    {
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();

        services.AddApiServices(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider
            .GetRequiredService<IOptions<HealthCheckServiceOptions>>()
            .Value;

        options.Registrations.Should().ContainSingle(registration =>
            registration.Name == "self" &&
            registration.Tags.Contains("live"));

        options.Registrations.Should().ContainSingle(registration =>
            registration.Name == "database" &&
            registration.Tags.Contains("ready"));
    }

    [Fact]
    public async Task AddApiServices_ShouldRejectRefreshTokensForBearerAuthentication()
    {
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();
        services.AddLogging();
        services.AddApiServices(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var generator = new JwtTokenGenerator(configuration);
        var (refreshToken, _) = generator.GenerateRefreshToken(
            Guid.NewGuid(),
            "rostik",
            "rostik@example.com",
            "client-1");

        var principal = new JwtSecurityTokenHandler().ValidateToken(
            refreshToken,
            options.TokenValidationParameters,
            out var securityToken);

        var context = new TokenValidatedContext(
            new DefaultHttpContext { RequestServices = provider },
            new AuthenticationScheme(
                JwtBearerDefaults.AuthenticationScheme,
                displayName: null,
                typeof(JwtBearerHandler)),
            options)
        {
            Principal = principal,
            SecurityToken = securityToken
        };

        await options.Events.TokenValidated(context);

        context.Result.Should().NotBeNull();
        context.Result!.Failure!.Message.Should().Contain("access");
    }

    [Theory]
    [InlineData(nameof(AuthController.Register))]
    [InlineData(nameof(AuthController.Login))]
    [InlineData(nameof(AuthController.Refresh))]
    public void AnonymousAuthEndpoints_ShouldUseAuthRateLimitPolicy(string actionName)
    {
        var attribute = typeof(AuthController)
            .GetMethod(actionName)!
            .GetCustomAttribute<EnableRateLimitingAttribute>();

        attribute.Should().NotBeNull();
        attribute!.PolicyName.Should().Be("auth");
    }

    [Fact]
    public void UseApiPipeline_ShouldMapLiveAndReadyHealthCheckEndpoints()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Production
        });

        builder.Configuration.AddInMemoryCollection(CreateValidSettings());
        builder.Services.AddApiServices(builder.Configuration);

        using var app = builder.Build();

        app.UseApiPipeline();

        var routePatterns = app.Services
            .GetRequiredService<IEnumerable<EndpointDataSource>>()
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .ToArray();

        routePatterns.Should().Contain("/health/live");
        routePatterns.Should().Contain("/health/ready");
    }

    [Fact]
    public void UseApiPipeline_ShouldEnableRateLimiterMiddleware()
    {
        var source = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "Api",
            "Configuration",
            "ApplicationBuilderExtensions.cs"));

        source.Should().Contain("UseRateLimiter(");
    }

    private static IConfiguration CreateConfiguration(params (string Key, string Value)[] values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(x => x.Key, x => (string?)x.Value))
            .Build();
    }

    private static IConfiguration CreateConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static IConfiguration CreateValidConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(CreateValidSettings())
            .Build();
    }

    private static Dictionary<string, string?> CreateValidSettings()
    {
        return new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=MSaverDb;Username=postgres",
            ["JwtSettings:Key"] = "0123456789abcdef0123456789abcdef",
            ["JwtSettings:Issuer"] = "MSaver",
            ["JwtSettings:Audience"] = "MSaverFrontend",
            ["Cors:FrontendOrigin"] = "http://localhost:4200",
            ["ExchangeRateApi:BaseUrl"] = "https://api.example.test/",
            ["ExchangeRateApi:ApiKey"] = "test-key"
        };
    }
}
