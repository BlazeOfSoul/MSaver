namespace server.Application.Features.ExchangeRates;

public sealed class ExchangeRatesResponse
{
    public List<Rate> Fiat { get; init; } = [];
    public List<Rate> Crypto { get; init; } = [];
}

public sealed record Rate(string Currency, decimal RateValue);