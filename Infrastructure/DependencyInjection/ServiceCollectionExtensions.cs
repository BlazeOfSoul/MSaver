using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using MSaver.Api.Auth;
using MSaver.Application.Services;
using MSaver.Infrastructure.Auth;
using MSaver.Infrastructure.Configuration;
using MSaver.Infrastructure.Persistence.Repositories;
using MSaver.Infrastructure.Services;

namespace MSaver.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITagService, TagService>();

        services.Configure<ExchangeRateApiOptions>(
            configuration.GetSection(ExchangeRateApiOptions.SectionName));

        services.AddHttpClient<IExchangeRateService, ExchangeRateApiService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ExchangeRateApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        // Auth / current user
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        return services;
    }
}