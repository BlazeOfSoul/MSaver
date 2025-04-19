using MediatR;

namespace server.Features.ExchangeRates;

public class GetExchangeRatesQuery : IRequest<ExchangeRatesResponse> { }