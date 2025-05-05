using server.Features.ExchangeRates;

namespace server.Domain.Interfaces;

public interface IExchangeRateService
{
    Task<ExchangeRatesResponse> GetExchangeRatesAsync();
}

