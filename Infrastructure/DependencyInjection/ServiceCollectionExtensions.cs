using Microsoft.AspNetCore.Identity;

using MSaver.Application.Abstractions.Auth;
using MSaver.Application.Abstractions.Services;
using MSaver.Application.Services;
using MSaver.Domain.Entities;
using MSaver.Domain.Repositories;

using MSaver.Infrastructure.Auth;
using MSaver.Infrastructure.ExchangeRate;
using MSaver.Infrastructure.Persistence;
using MSaver.Infrastructure.Persistence.Repositories;

namespace MSaver.Infrastructure.DependencyInjection;

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
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        // External
        services.AddHttpClient<IExchangeRateService, ExchangeRateService>();

        return services;
    }
}