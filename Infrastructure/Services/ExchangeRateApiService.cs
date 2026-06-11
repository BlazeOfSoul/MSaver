using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MSaver.Infrastructure.Configuration;

namespace MSaver.Infrastructure.Services;

public sealed class ExchangeRateApiService(
    HttpClient httpClient,
    IOptions<ExchangeRateApiOptions> options,
    IMemoryCache cache,
    ILogger<ExchangeRateApiService> logger) : IExchangeRateService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ExchangeRateApiOptions _options = options.Value;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<ExchangeRateApiService> _logger = logger;

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
        {
            _logger.LogWarning("Exchange rate API key is not configured");
            throw new InvalidOperationException("Не настроен ExchangeRateApi:ApiKey.");
        }

        var from = fromCurrencyCode.Trim().ToUpperInvariant();
        var to = toCurrencyCode.Trim().ToUpperInvariant();

        if (from == to)
        {
            _logger.LogDebug(
                "Exchange rate skipped because currencies match for {CurrencyCode}",
                from);

            return 1m;
        }

        var cacheKey = GetCacheKey(from, to);

        if (_cache.TryGetValue<decimal>(cacheKey, out var cachedRate))
        {
            _logger.LogDebug(
                "Exchange rate cache hit for {FromCurrencyCode} to {ToCurrencyCode}",
                from,
                to);

            return cachedRate;
        }

        _logger.LogInformation(
            "Fetching exchange rate for {FromCurrencyCode} to {ToCurrencyCode}",
            from,
            to);

        var rate = await FetchRateFromApiAsync(from, to, cancellationToken);

        CacheRate(from, to, rate);
        CacheRate(to, from, 1m / rate);

        _logger.LogInformation(
            "Exchange rate fetched and cached for {FromCurrencyCode} to {ToCurrencyCode}",
            from,
            to);

        return rate;
    }

    private async Task<decimal> FetchRateFromApiAsync(
        string from,
        string to,
        CancellationToken cancellationToken)
    {
        var url = $"{_options.ApiKey}/pair/{from}/{to}";

        using var httpResponse = await _httpClient.GetAsync(url, cancellationToken);
        var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Exchange rate provider returned HTTP error {StatusCode} for {FromCurrencyCode} to {ToCurrencyCode}",
                (int)httpResponse.StatusCode,
                from,
                to);

            throw CreateHttpException(httpResponse.StatusCode, httpResponse.ReasonPhrase, content);
        }

        var response = Deserialize<PairExchangeRateResponse>(content,
            "Не удалось разобрать успешный ответ сервиса курсов валют.");

        if (!string.Equals(response.Result, "success", StringComparison.OrdinalIgnoreCase))
        {
            var apiError = TryDeserialize<ExchangeRateApiErrorResponse>(content);

            _logger.LogWarning(
                "Exchange rate provider returned API error {ErrorType} for {FromCurrencyCode} to {ToCurrencyCode}",
                apiError?.ErrorType,
                from,
                to);

            throw new InvalidOperationException(
                apiError is null
                    ? "Внешний сервис курсов валют вернул ошибку."
                    : MapApiError(apiError.ErrorType));
        }

        if (response.ConversionRate <= 0)
        {
            _logger.LogWarning(
                "Exchange rate provider returned invalid rate {ConversionRate} for {FromCurrencyCode} to {ToCurrencyCode}",
                response.ConversionRate,
                from,
                to);

            throw new InvalidOperationException("Внешний сервис вернул некорректный курс валют.");
        }

        return response.ConversionRate;
    }

    private void CacheRate(string from, string to, decimal rate)
    {
        _cache.Set(
            GetCacheKey(from, to),
            rate,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = GetCacheDuration()
            });
    }

    private TimeSpan GetCacheDuration()
    {
        var hours = _options.CacheDurationHours > 0 ? _options.CacheDurationHours : 24;

        return TimeSpan.FromHours(hours);
    }

    private static string GetCacheKey(string from, string to) =>
        $"exchange-rate:{from}:{to}";

    private static InvalidOperationException CreateHttpException(
        HttpStatusCode statusCode,
        string? reasonPhrase,
        string? content)
    {
        var apiError = TryDeserialize<ExchangeRateApiErrorResponse>(content);

        if (apiError is not null)
            return new InvalidOperationException(MapApiError(apiError.ErrorType));

        return new InvalidOperationException(
            $"Ошибка HTTP при обращении к сервису курсов валют: {(int)statusCode} ({reasonPhrase}).");
    }

    private static T Deserialize<T>(string content, string errorMessage)
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(content);

            return result ?? throw new InvalidOperationException(errorMessage);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(errorMessage, ex);
        }
    }

    private static T? TryDeserialize<T>(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(content);
        }
        catch
        {
            return default;
        }
    }

    private static string MapApiError(string? errorType) =>
        errorType?.ToLowerInvariant() switch
        {
            "unsupported-code" => "Сервис курсов валют не поддерживает указанный код валюты.",
            "unknown-code" => "Сервис курсов валют не распознал код валюты.",
            "malformed-request" => "Сервис курсов валют получил некорректный запрос.",
            "invalid-key" => "Указан некорректный API-ключ сервиса курсов валют.",
            "inactive-account" => "Аккаунт сервиса курсов валют не активирован.",
            "quota-reached" => "Превышен лимит запросов к сервису курсов валют.",
            null or "" => "Внешний сервис курсов валют вернул ошибку.",
            _ => $"Внешний сервис курсов валют вернул ошибку: {errorType}."
        };

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
    }

    private sealed class ExchangeRateApiErrorResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; init; } = string.Empty;

        [JsonPropertyName("error-type")]
        public string ErrorType { get; init; } = string.Empty;
    }
}
