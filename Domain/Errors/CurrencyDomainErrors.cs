namespace MSaver.Domain.Errors;

public static class CurrencyDomainErrors
{
    public static readonly DomainError DefaultCurrencyNotFound =
        DomainError.NotFound(
            code: "Currency.DefaultNotFound",
            message: "Валюта по умолчанию не найдена.");
}