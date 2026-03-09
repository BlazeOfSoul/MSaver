using server.Application.Features.ExchangeRates;

namespace server.Application.Abstractions.Services;

public interface IExchangeRateService
{
    Task<ExchangeRatesResponse> GetExchangeRatesAsync(CancellationToken cancellationToken = default);
}
