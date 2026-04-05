namespace MSaver.Domain.Entities;

public sealed class ExchangeRate : Entity
{
    private ExchangeRate()
    {
        Source = null!;
        FromCurrency = null!;
        ToCurrency = null!;
    }

    public Guid FromCurrencyId { get; private set; }
    public Currency FromCurrency { get; private set; } = null!;

    public Guid ToCurrencyId { get; private set; }
    public Currency ToCurrency { get; private set; } = null!;

    public decimal Rate { get; private set; }

    public string Source { get; private set; }

    public DateTime FetchedAtUtc { get; private set; }

    public static ExchangeRate Create(
        Guid fromCurrencyId,
        Guid toCurrencyId,
        decimal rate,
        string source,
        DateTime fetchedAtUtc)
    {
        if (fromCurrencyId == Guid.Empty)
            throw new DomainException(ExchangeRateDomainErrors.FromCurrencyIdRequired);

        if (toCurrencyId == Guid.Empty)
            throw new DomainException(ExchangeRateDomainErrors.ToCurrencyIdRequired);

        if (fromCurrencyId == toCurrencyId)
            throw new DomainException(ExchangeRateDomainErrors.CurrenciesMustBeDifferent);

        if (rate <= 0)
            throw new DomainException(ExchangeRateDomainErrors.RateMustBePositive);

        if (string.IsNullOrWhiteSpace(source))
            throw new DomainException(ExchangeRateDomainErrors.SourceRequired);

        return new ExchangeRate
        {
            FromCurrencyId = fromCurrencyId,
            ToCurrencyId = toCurrencyId,
            Rate = rate,
            Source = source.Trim(),
            FetchedAtUtc = fetchedAtUtc
        };
    }

    public void Update(decimal rate, string source, DateTime fetchedAtUtc)
    {
        if (rate <= 0)
            throw new DomainException(ExchangeRateDomainErrors.RateMustBePositive);

        if (string.IsNullOrWhiteSpace(source))
            throw new DomainException(ExchangeRateDomainErrors.SourceRequired);

        Rate = rate;
        Source = source.Trim();
        FetchedAtUtc = fetchedAtUtc;
    }
}