using Microsoft.Extensions.Caching.Memory;
using server.Domain.Interfaces;
using server.Features.ExchangeRates;
using server.Models.ExchangeRate;

namespace server.Domain.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private const string CacheKey = "exchange_rates";

    public ExchangeRateService(IMemoryCache cache, HttpClient httpClient)
    {
        _cache = cache;
        _httpClient = httpClient;
    }

    public async Task<ExchangeRatesResponse> GetExchangeRatesAsync()
    {
        if (_cache.TryGetValue(CacheKey, out ExchangeRatesResponse cachedRates))
        {
            return cachedRates;
        }

        try
        {
            var usd = await _httpClient.GetFromJsonAsync<NbrbRate>("https://api.nbrb.by/exrates/rates/431");
            var eur = await _httpClient.GetFromJsonAsync<NbrbRate>("https://api.nbrb.by/exrates/rates/451");
            var rub = await _httpClient.GetFromJsonAsync<NbrbRate>("https://api.nbrb.by/exrates/rates/456");

            if (usd == null || eur == null || rub == null)
            {
                throw new Exception("Не удалось получить курсы валют от NBRB");
            }

            var crypto = await _httpClient.GetFromJsonAsync<CoinGeckoResponse>(
                "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin,ethereum,solana&vs_currencies=usd");

            if (crypto == null)
            {
                throw new Exception("Не удалось получить курсы криптовалют от CoinGecko");
            }

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
                    new("Bitcoin (BTC)", crypto.Bitcoin.Usd),
                    new("Ethereum (ETH)", crypto.Ethereum.Usd),
                    new("Solana (SOL)", crypto.Solana.Usd),
                }
            };

            _cache.Set(CacheKey, result, TimeSpan.FromDays(1));
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при получении курсов валют: {ex.Message}", ex);
        }
    }
}
