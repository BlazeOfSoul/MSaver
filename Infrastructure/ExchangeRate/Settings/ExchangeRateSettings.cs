namespace MSaver.Infrastructure.ExchangeRate.Settings;

public sealed class ExchangeRateSettings
{
    public NbrbSettings Nbrb { get; set; } = new();
    public string CoinGecko { get; set; } = string.Empty;
}
