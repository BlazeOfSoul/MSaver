using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using MSaver.Api.Common;
using MSaver.Application.Features.Auth.Register;
using MSaver.Infrastructure;
using MSaver.Infrastructure.Configuration;
using MSaver.Infrastructure.DependencyInjection;
using MSaver.Infrastructure.Health;
using MSaver.Infrastructure.Persistence.Interceptors;

namespace MSaver.Api.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ValidateRequiredConfiguration(configuration);
        var frontendOrigins = GetFrontendOrigins(configuration);

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddValidatorsFromAssemblyContaining<RegisterRequest>();

        services.AddMemoryCache();
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MSaver API",
                Version = "v1"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter only the JWT token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.AddScoped<AuditableEntitiesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntitiesInterceptor>());
        });

        services.AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy(),
                tags: ["live"])
            .AddCheck<ApplicationDbContextHealthCheck>(
                "database",
                tags: ["ready"]);

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                policy => policy
                    .WithOrigins(frontendOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        services.AddApplicationServices(configuration);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtSettings.GetIssuer(configuration),
                    ValidateAudience = true,
                    ValidAudience = JwtSettings.GetAudience(configuration),
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = JwtSettings.CreateSigningKey(configuration)
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var tokenType = context.Principal?
                            .FindFirst(JwtRegisteredClaimNames.Typ)?
                            .Value;

                        if (!string.Equals(tokenType, "access", StringComparison.Ordinal))
                            context.Fail("Only access tokens can be used for bearer authentication.");

                        return Task.CompletedTask;
                    },

                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearer");

                        logger.LogError(context.Exception, "JWT authentication failed");
                        return Task.CompletedTask;
                    },

                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        return ApiErrorWriter.WriteAsync(
                            context.Response,
                            StatusCodes.Status401Unauthorized,
                            ApiErrorFactory.Unauthorized(),
                            context.HttpContext.RequestAborted);
                    }
                };
            });

        return services;
    }

    private static void ValidateRequiredConfiguration(IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection")))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection configuration is missing.");

        JwtSettings.GetIssuer(configuration);
        JwtSettings.GetAudience(configuration);
        JwtSettings.GetAccessTokenMinutes(configuration);
        JwtSettings.GetRefreshTokenDays(configuration);
        JwtSettings.CreateSigningKey(configuration);

        ValidateExchangeRateApiConfiguration(configuration);
    }

    private static void ValidateExchangeRateApiConfiguration(IConfiguration configuration)
    {
        var apiKey = configuration[$"{ExchangeRateApiOptions.SectionName}:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("ExchangeRateApi:ApiKey configuration is missing.");

        var baseUrl = configuration[$"{ExchangeRateApiOptions.SectionName}:BaseUrl"];
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "ExchangeRateApi:BaseUrl must be an absolute HTTP or HTTPS URL.");
        }

        ValidatePositiveOptionalInt(configuration, "CacheDurationHours");
        ValidatePositiveOptionalInt(configuration, "TimeoutSeconds");
    }

    private static void ValidatePositiveOptionalInt(IConfiguration configuration, string name)
    {
        var key = $"{ExchangeRateApiOptions.SectionName}:{name}";
        var rawValue = configuration[key];

        if (string.IsNullOrWhiteSpace(rawValue))
            return;

        if (!int.TryParse(rawValue, out var value) || value <= 0)
            throw new InvalidOperationException($"{key} must be a positive integer.");
    }

    private static string[] GetFrontendOrigins(IConfiguration configuration)
    {
        var configuredOrigins = configuration
            .GetSection("Cors:FrontendOrigins")
            .Get<string[]>() ?? [];

        var frontendOrigin = configuration["Cors:FrontendOrigin"];
        var frontendOrigins = configuredOrigins
            .Append(frontendOrigin)
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (frontendOrigins.Length == 0)
            throw new InvalidOperationException("Cors:FrontendOrigin configuration is missing.");

        if (frontendOrigins.Any(origin => origin == "*"))
            throw new InvalidOperationException("Cors configuration cannot contain wildcard origins.");

        if (frontendOrigins.Any(origin =>
                !Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
        {
            throw new InvalidOperationException("Cors frontend origins must be absolute HTTP or HTTPS URLs.");
        }

        return frontendOrigins;
    }
}
