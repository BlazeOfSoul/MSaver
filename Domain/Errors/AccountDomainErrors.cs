namespace MSaver.Domain.Errors;

public static class AccountDomainErrors
{
    public static readonly DomainError UserIdRequired =
        DomainError.Validation(
            code: "Account.UserIdRequired",
            message: "Идентификатор пользователя обязателен.",
            field: "userId");

    public static readonly DomainError AccountIdRequired =
        DomainError.Validation(
            code: "Account.IdRequired",
            message: "Идентификатор счёта обязателен.",
            field: "id");

    public static readonly DomainError CurrencyCodeRequired =
        DomainError.Validation(
            code: "Account.CurrencyCodeRequired",
            message: "Код валюты обязателен.",
            field: "currencyCode");

    public static readonly DomainError NameRequired =
        DomainError.Validation(
            code: "Account.NameRequired",
            message: "Название счёта обязательно.",
            field: "name");

    public static readonly DomainError InitialBalanceNegative =
        DomainError.Validation(
            code: "Account.InitialBalanceNegative",
            message: "Начальный баланс не может быть отрицательным.",
            field: "initialBalance");

    public static readonly DomainError InitialBalancePrecisionInvalid =
        DomainError.Validation(
            code: "Account.InitialBalancePrecisionInvalid",
            message: "Начальный баланс содержит больше знаков после запятой, чем поддерживает валюта счёта.",
            field: "initialBalance");

    public static readonly DomainError CurrencyNotFound =
        DomainError.Validation(
            code: "Account.CurrencyNotFound",
            message: "Валюта не найдена.",
            field: "currencyCode");

    public static readonly DomainError NameAlreadyExists =
        DomainError.Conflict(
            code: "Account.NameAlreadyExists",
            message: "Счёт с таким названием уже существует.",
            field: "name");

    public static readonly DomainError NotFound =
        DomainError.NotFound(
            code: "Account.NotFound",
            message: "Счёт не найден.");

    public static readonly DomainError AlreadyActive =
        DomainError.Validation(
            code: "Account.AlreadyActive",
            message: "Счёт уже активен.",
            field: "id");

    public static readonly DomainError PrimaryAccountAlreadyExists =
        DomainError.Conflict(
            code: "Account.PrimaryAccountAlreadyExists",
            message: "Основной счёт уже существует.");

    public static readonly DomainError PrimaryAccountCannotBeArchived =
        DomainError.Conflict(
            code: "Account.PrimaryAccountCannotBeArchived",
            message: "Основной счёт не может быть архивирован.");
}
