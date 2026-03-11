using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using server.Application.Common.Results;
using server.Application.Features.ExchangeRates;
using server.Application.Services.Interfaces;
using server.Domain.Errors;
using server.Infrastructure.ExchangeRate.Models;
using server.Infrastructure.ExchangeRate.Settings;

namespace server.Infrastructure.ExchangeRate;

public sealed class ExchangeRateService : IExchangeRateService
{
    private const string CacheKey = "exchange_rates";

    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly ExchangeRateSettings _settings;

    public ExchangeRateService(
        IMemoryCache cache,
        HttpClient httpClient,
        IOptions<ExchangeRateSettings> settings)
    {
        _cache = cache;
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<Result<ExchangeRatesResponse>> GetExchangeRatesAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out ExchangeRatesResponse? cachedRates))
            return Result<ExchangeRatesResponse>.Success(cachedRates!);

        var usd = await _httpClient.GetFromJsonAsync<NbrbRate>(_settings.Nbrb.Usd, cancellationToken);
        var eur = await _httpClient.GetFromJsonAsync<NbrbRate>(_settings.Nbrb.Eur, cancellationToken);
        var rub = await _httpClient.GetFromJsonAsync<NbrbRate>(_settings.Nbrb.Rub, cancellationToken);

        if (usd is null || eur is null || rub is null)
        {
            return Result<ExchangeRatesResponse>.Failure(ExchangeRateDomainErrors.NbrbRateNotFound);
        }

        // var crypto = await _httpClient.GetFromJsonAsync<CoinGeckoResponse>(_settings.CoinGecko, cancellationToken);
        // if (crypto is null)
        // {
        //     return Result<ExchangeRatesResponse>.Failure(ExchangeRateDomainErrors.CoinGeckoNotFound);
        // }

        var result = new ExchangeRatesResponse
        {
            Fiat = new List<Rate>
            {
                new("USD", usd.Cur_OfficialRate),
                new("EUR", eur.Cur_OfficialRate),
                new("RUB (за 100)", rub.Cur_OfficialRate),
            },
            Crypto = new List<Rate>
            {
                new("Bitcoin (BTC)", 1),
                new("Ethereum (ETH)", 1),
                new("Solana (SOL)", 1),
            }
        };

        _cache.Set(CacheKey, result, TimeSpan.FromDays(1));

        return Result<ExchangeRatesResponse>.Success(result);
    }
}
