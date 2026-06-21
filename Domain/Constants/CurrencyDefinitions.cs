namespace MSaver.Domain.Constants;

public static class CurrencyDefinitions
{
    private static readonly IReadOnlyDictionary<string, CurrencyDefinition> _all =
        new Dictionary<string, CurrencyDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["AMD"] = new("AMD", "Armenian Dram", "֏", 2),
            ["ARS"] = new("ARS", "Argentine Peso", "$", 2),
            ["AZN"] = new("AZN", "Azerbaijani Manat", "₼", 2),
            ["BRL"] = new("BRL", "Brazilian Real", "R$", 2),
            ["BYN"] = new("BYN", "Belarusian Ruble", "Br", 2),
            ["CHF"] = new("CHF", "Swiss Franc", "Fr", 2),
            ["CLP"] = new("CLP", "Chilean Peso", "$", 0),
            ["CNY"] = new("CNY", "Chinese Yuan", "¥", 2),
            ["COP"] = new("COP", "Colombian Peso", "$", 2),
            ["CZK"] = new("CZK", "Czech Koruna", "Kč", 2),
            ["DKK"] = new("DKK", "Danish Krone", "kr", 2),
            ["EUR"] = new("EUR", "Euro", "€", 2),
            ["GBP"] = new("GBP", "British Pound", "£", 2),
            ["GEL"] = new("GEL", "Georgian Lari", "₾", 2),
            ["HKD"] = new("HKD", "Hong Kong Dollar", "HK$", 2),
            ["INR"] = new("INR", "Indian Rupee", "₹", 2),
            ["JPY"] = new("JPY", "Japanese Yen", "¥", 0),
            ["KRW"] = new("KRW", "South Korean Won", "₩", 0),
            ["KZT"] = new("KZT", "Kazakhstani Tenge", "₸", 2),
            ["NOK"] = new("NOK", "Norwegian Krone", "kr", 2),
            ["PEN"] = new("PEN", "Peruvian Sol", "S/", 2),
            ["PLN"] = new("PLN", "Polish Zloty", "zł", 2),
            ["RUB"] = new("RUB", "Russian Ruble", "₽", 2),
            ["SEK"] = new("SEK", "Swedish Krona", "kr", 2),
            ["SGD"] = new("SGD", "Singapore Dollar", "S$", 2),
            ["THB"] = new("THB", "Thai Baht", "฿", 2),
            ["UAH"] = new("UAH", "Ukrainian Hryvnia", "₴", 2),
            ["USD"] = new("USD", "US Dollar", "$", 2),
            ["UYU"] = new("UYU", "Uruguayan Peso", "$U", 2)
        };

    public static bool Exists(string code) =>
        _all.ContainsKey(code);

    public static CurrencyDefinition Get(string code)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        if (!_all.TryGetValue(normalizedCode, out var currency))
            throw new DomainException(AccountDomainErrors.CurrencyNotFound);

        return currency;
    }
}

public sealed record CurrencyDefinition(
    string Code,
    string Name,
    string Symbol,
    short Precision);
