using server.Repositories;
using server.Repositories.Interfaces;
using server.Domain.Interfaces;
using server.Domain.Services;

namespace server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBalanceRepository, BalanceRepository>();

        // Services
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();

        return services;
    }
}
