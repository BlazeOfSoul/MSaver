using MSaver.Domain.Constants;

namespace MSaver.Domain.ValueObjects;

public sealed record CurrencyCode
{
    public string Value { get; }

    private CurrencyCode(string value)
    {
        Value = value;
    }

    public static CurrencyCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(AccountDomainErrors.CurrencyCodeRequired);

        var normalizedCode = code.Trim().ToUpperInvariant();

        if (!CurrencyDefinitions.Exists(normalizedCode))
            throw new DomainException(AccountDomainErrors.CurrencyNotFound);

        return new CurrencyCode(normalizedCode);
    }

    public override string ToString() => Value;

    public static implicit operator string(CurrencyCode code) => code.Value;
}