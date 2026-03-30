using server.Application.Common.Results;
using server.Application.Features.ExchangeRates;

namespace server.Application.Abstractions.Services;

public interface IExchangeRateService
{
    Task<Result<ExchangeRatesResponse>> GetExchangeRatesAsync(CancellationToken cancellationToken = default);
}
