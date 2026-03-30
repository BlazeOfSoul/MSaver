using server.Application.Abstractions.Auth;
using server.Application.Abstractions.Services;
using server.Application.Services;

using server.Domain.Repositories;

using server.Infrastructure.Auth;
using server.Infrastructure.ExchangeRate;
using server.Infrastructure.Persistence;
using server.Infrastructure.Persistence.Repositories;

namespace server.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBalanceRepository, BalanceRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBalanceService, BalanceService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITransactionService, TransactionService>();

        // Auth / current user
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // External
        services.AddHttpClient<IExchangeRateService, ExchangeRateService>();

        return services;
    }
}