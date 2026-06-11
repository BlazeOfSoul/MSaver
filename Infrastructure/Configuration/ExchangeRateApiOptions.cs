namespace MSaver.Infrastructure.Configuration;

public sealed class ExchangeRateApiOptions
{
    public const string SectionName = "ExchangeRateApi";
    public const int DefaultTimeoutSeconds = 10;

    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public int CacheDurationHours { get; init; } = 24;
    public int TimeoutSeconds { get; init; } = DefaultTimeoutSeconds;
}
