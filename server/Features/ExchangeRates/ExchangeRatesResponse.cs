namespace server.Features.ExchangeRates;

public class ExchangeRatesResponse
{
    public List<Rate> Fiat { get; set; } = [];
    public List<Rate> Crypto { get; set; } = [];
}

public record Rate(string Currency, decimal RateValue);