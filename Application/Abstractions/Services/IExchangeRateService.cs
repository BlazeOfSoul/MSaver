using MSaver.Application.Features.ExchangeRates;

namespace MSaver.Application.Abstractions.Services;

public interface IExchangeRateService
{
    Task<Result<ExchangeRatesResponse>> GetExchangeRatesAsync(CancellationToken cancellationToken = default);
}
