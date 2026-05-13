namespace MSaver.Domain.Errors;

public static class CurrencyDomainErrors
{
    public static readonly DomainError CodeRequired =
        DomainError.Validation(
            code: "Currency.CodeRequired",
            message: "Код валюты обязателен.");

    public static readonly DomainError UnsupportedCode =
        DomainError.Validation(
            code: "Currency.UnsupportedCode",
            message: "Указанный код валюты не поддерживается.");

    public static readonly DomainError DefaultCurrencyNotFound =
        DomainError.NotFound(
            code: "Currency.DefaultNotFound",
            message: "Валюта по умолчанию не найдена.");
}