namespace MSaver.Infrastructure.Configuration;

public sealed class ExchangeRateApiOptions
{
    public const string SectionName = "ExchangeRateApi";

    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
}