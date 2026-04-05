namespace MSaver.Domain.Errors;

public static class ExchangeRateDomainErrors
{
    public static readonly DomainError FromCurrencyIdRequired =
        DomainError.Validation(
            code: "ExchangeRate.FromCurrencyIdRequired",
            message: "Идентификатор исходной валюты обязателен.",
            field: "fromCurrencyId");

    public static readonly DomainError ToCurrencyIdRequired =
        DomainError.Validation(
            code: "ExchangeRate.ToCurrencyIdRequired",
            message: "Идентификатор целевой валюты обязателен.",
            field: "toCurrencyId");

    public static readonly DomainError CurrenciesMustBeDifferent =
        DomainError.Validation(
            code: "ExchangeRate.CurrenciesMustBeDifferent",
            message: "Исходная и целевая валюта должны отличаться.");

    public static readonly DomainError RateMustBePositive =
        DomainError.Validation(
            code: "ExchangeRate.RateMustBePositive",
            message: "Курс обмена должен быть больше нуля.",
            field: "rate");

    public static readonly DomainError SourceRequired =
        DomainError.Validation(
            code: "ExchangeRate.SourceRequired",
            message: "Источник курса обязателен.",
            field: "source");

    public static readonly DomainError ExchangeRateNotFound =
        DomainError.NotFound(
            code: "ExchangeRate.NotFound",
            message: "Курс обмена не найден.");

    public static readonly DomainError FromCurrencyNotFound =
        DomainError.NotFound(
            code: "ExchangeRate.FromCurrencyNotFound",
            message: "Исходная валюта не найдена.");

    public static readonly DomainError ToCurrencyNotFound =
        DomainError.NotFound(
            code: "ExchangeRate.ToCurrencyNotFound",
            message: "Целевая валюта не найдена.");
}