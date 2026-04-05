namespace MSaver.Domain.Errors;

public static class AccountDomainErrors
{
    public static readonly DomainError UserIdRequired =
        DomainError.Validation(
            code: "Account.UserIdRequired",
            message: "Идентификатор пользователя обязателен.",
            field: "userId");

    public static readonly DomainError CurrencyIdRequired =
        DomainError.Validation(
            code: "Account.CurrencyIdRequired",
            message: "Идентификатор валюты обязателен.",
            field: "currencyId");

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

    public static readonly DomainError CurrencyNotFound =
        DomainError.NotFound(
            code: "Account.CurrencyNotFound",
            message: "Валюта не найдена.");

    public static readonly DomainError NameAlreadyExists =
        DomainError.Conflict(
            code: "Account.NameAlreadyExists",
            message: "Счёт с таким названием уже существует.",
            field: "name");

    public static readonly DomainError AccountNotFound =
        DomainError.NotFound(
            code: "Account.NotFound",
            message: "Счёт не найден.");

    public static readonly DomainError AccountAlreadyArchived =
        DomainError.Validation(
            code: "Account.AlreadyArchived",
            message: "Счёт уже находится в архиве.",
            field: "id");

    public static readonly DomainError AccountAlreadyActive =
        DomainError.Validation(
            code: "Account.AlreadyActive",
            message: "Счёт уже активен.",
            field: "id");
}