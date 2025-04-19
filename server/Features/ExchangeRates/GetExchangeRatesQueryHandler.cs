using MediatR;
using server.Domain.Interfaces;

namespace server.Features.ExchangeRates;

public class GetExchangeRatesQueryHandler : IRequestHandler<GetExchangeRatesQuery, ExchangeRatesResponse>
{
    private readonly IExchangeRateService _exchangeRateService;

    public GetExchangeRatesQueryHandler(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    public async Task<ExchangeRatesResponse> Handle(GetExchangeRatesQuery request, CancellationToken cancellationToken)
    {
        return await _exchangeRateService.GetExchangeRatesAsync();
    }
}
