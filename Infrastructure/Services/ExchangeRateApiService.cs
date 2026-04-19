using System.Text.Json.Serialization;

using Microsoft.Extensions.Options;

using MSaver.Infrastructure.Configuration;

namespace MSaver.Infrastructure.Services;

public sealed class ExchangeRateApiService(
    HttpClient httpClient,
    IOptions<ExchangeRateApiOptions> options) : IExchangeRateService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ExchangeRateApiOptions _options = options.Value;

    public async Task<decimal> GetRateAsync(
        string fromCurrencyCode,
        string toCurrencyCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fromCurrencyCode))
            throw new ArgumentException("Код исходной валюты обязателен.", nameof(fromCurrencyCode));

        if (string.IsNullOrWhiteSpace(toCurrencyCode))
            throw new ArgumentException("Код целевой валюты обязателен.", nameof(toCurrencyCode));

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Не настроен ExchangeRateApi:ApiKey.");

        var from = fromCurrencyCode.Trim().ToUpperInvariant();
        var to = toCurrencyCode.Trim().ToUpperInvariant();

        if (from == to)
            return 1m;

        var url = $"{_options.ApiKey}/pair/{from}/{to}";

        var response = await _httpClient.GetFromJsonAsync<PairExchangeRateResponse>(
            url,
            cancellationToken);

        if (response is null)
            throw new InvalidOperationException("Не удалось получить ответ от сервиса курсов валют.");

        if (!string.Equals(response.Result, "success", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Внешний сервис курсов валют вернул ошибку.");

        if (response.ConversionRate <= 0)
            throw new InvalidOperationException("Внешний сервис вернул некорректный курс валют.");

        return response.ConversionRate;
    }

    private sealed class PairExchangeRateResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; init; } = string.Empty;

        [JsonPropertyName("base_code")]
        public string BaseCode { get; init; } = string.Empty;

        [JsonPropertyName("target_code")]
        public string TargetCode { get; init; } = string.Empty;

        [JsonPropertyName("conversion_rate")]
        public decimal ConversionRate { get; init; }

        [JsonPropertyName("time_last_update_unix")]
        public long TimeLastUpdateUnix { get; init; }

        [JsonPropertyName("time_last_update_utc")]
        public string TimeLastUpdateUtc { get; init; } = string.Empty;

        [JsonPropertyName("time_next_update_unix")]
        public long TimeNextUpdateUnix { get; init; }

        [JsonPropertyName("time_next_update_utc")]
        public string TimeNextUpdateUtc { get; init; } = string.Empty;
    }
}