namespace MSaver.Application.Abstractions.Services;

public interface IExchangeRateService
{
    Task<decimal> GetRateAsync(
        string fromCurrencyCode,
        string toCurrencyCode,
        CancellationToken cancellationToken = default);
}