using System.Text.Json.Serialization;

namespace server.Infrastructure.ExchangeRate.Models;

public sealed class CoinGeckoResponse
{
    [JsonPropertyName("bitcoin")]
    public CoinPrice Bitcoin { get; set; } = null!;

    [JsonPropertyName("ethereum")]
    public CoinPrice Ethereum { get; set; } = null!;

    [JsonPropertyName("solana")]
    public CoinPrice Solana { get; set; } = null!;
}

public sealed class CoinPrice
{
    [JsonPropertyName("usd")]
    public decimal Usd { get; set; }
}
