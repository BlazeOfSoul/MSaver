using System.Text.Json.Serialization;

namespace server.Models.ExchangeRate;

public class CryptoRate
{
    [JsonPropertyName("usd")]
    public decimal Usd { get; set; }
}
