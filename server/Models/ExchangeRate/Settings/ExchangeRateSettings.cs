namespace server.Models.ExchangeRate.Settings;

public class ExchangeRateSettings
{
    public NbrbSettings NBRB { get; set; }
    public string CoinGecko { get; set; } = string.Empty;
}
