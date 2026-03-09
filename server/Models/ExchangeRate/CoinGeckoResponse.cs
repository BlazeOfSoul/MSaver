using System.Text.Json.Serialization;

namespace server.Models.ExchangeRate;

public class CoinGeckoResponse
{
    [JsonPropertyName("bitcoin")]
    public CoinPrice Bitcoin { get; set; }

    [JsonPropertyName("ethereum")]
    public CoinPrice Ethereum { get; set; }

    [JsonPropertyName("solana")]
    public CoinPrice Solana { get; set; }
}

public class CoinPrice
{
    [JsonPropertyName("usd")]
    public decimal Usd { get; set; }
}
