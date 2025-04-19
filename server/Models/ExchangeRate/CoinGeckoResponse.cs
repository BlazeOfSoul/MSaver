using System.Text.Json.Serialization;

namespace server.Models.ExchangeRate;

public class CoinGeckoResponse
{
    [JsonPropertyName("bitcoin")]
    public CryptoRate Bitcoin { get; set; }

    [JsonPropertyName("ethereum")]
    public CryptoRate Ethereum { get; set; }

    [JsonPropertyName("solana")]
    public CryptoRate Solana { get; set; }
}

public class CryptoRate
{
    [JsonPropertyName("usd")]
    public decimal Usd { get; set; }
}
