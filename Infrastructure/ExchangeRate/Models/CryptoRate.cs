using System.Text.Json.Serialization;

namespace MSaver.Infrastructure.ExchangeRate.Models;

public sealed class CryptoRate
{
    [JsonPropertyName("usd")]
    public decimal Usd
    {
        get; set;
    }
}
