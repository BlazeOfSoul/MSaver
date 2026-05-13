namespace MSaver.Domain.Constants;

public static class CurrencyDefinitions
{
    private static readonly IReadOnlyDictionary<string, CurrencyDefinition> _all =
        new Dictionary<string, CurrencyDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["USD"] = new("USD", "US Dollar", "$", 2),
            ["EUR"] = new("EUR", "Euro", "€", 2),
            ["BYN"] = new("BYN", "Belarusian Ruble", "Br", 2),
            ["RUB"] = new("RUB", "Russian Ruble", "₽", 2)
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