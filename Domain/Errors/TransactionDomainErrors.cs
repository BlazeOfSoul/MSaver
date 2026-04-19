namespace MSaver.Domain.Errors;

public static class TransactionDomainErrors
{
    public static readonly DomainError UserIdRequired =
        DomainError.Validation(
            code: "Transaction.UserIdRequired",
            message: "Идентификатор пользователя обязателен.",
            field: "userId");

    public static readonly DomainError AccountIdRequired =
        DomainError.Validation(
            code: "Transaction.AccountIdRequired",
            message: "Идентификатор счёта обязателен.",
            field: "accountId");

    public static readonly DomainError CategoryIdRequired =
        DomainError.Validation(
            code: "Transaction.CategoryIdRequired",
            message: "Идентификатор категории обязателен.",
            field: "categoryId");

    public static readonly DomainError AmountMustNotBeZero =
        DomainError.Validation(
            code: "Transaction.AmountMustNotBeZero",
            message: "Сумма транзакции не может быть равна нулю.",
            field: "amount");

    public static readonly DomainError AmountSignMismatchWithCategoryType =
        DomainError.Validation(
            code: "Transaction.AmountSignMismatchWithCategoryType",
            message: "Знак суммы не соответствует типу категории.",
            field: "amount");

    public static readonly DomainError TransferIdRequired =
        DomainError.Validation(
            code: "Transaction.TransferIdRequired",
            message: "Идентификатор перевода обязателен.",
            field: "transferId");

    public static readonly DomainError TransferRateMustBePositive =
        DomainError.Validation(
            code: "Transaction.TransferRateMustBePositive",
            message: "Курс перевода должен быть больше нуля.",
            field: "transferRate");

    public static readonly DomainError TransferAccountsMustBeDifferent =
        DomainError.Validation(
            code: "Transaction.TransferAccountsMustBeDifferent",
            message: "Счета списания и зачисления должны отличаться.",
            field: "toAccountId");

    public static readonly DomainError TransferAmountMustBeGreaterThanZero =
        DomainError.Validation(
            code: "Transaction.TransferAmountMustBeGreaterThanZero",
            message: "Сумма перевода должна быть больше нуля.",
            field: "amount");

    public static readonly DomainError CategoryNotFound =
        DomainError.NotFound(
            code: "Transaction.CategoryNotFound",
            message: "Категория не найдена.");

    public static readonly DomainError AccountNotFound =
        DomainError.NotFound(
            code: "Transaction.AccountNotFound",
            message: "Счёт не найден.");

    public static readonly DomainError CurrencyNotFound =
        DomainError.NotFound(
            code: "Transaction.CurrencyNotFound",
            message: "Валюта не найдена.");

    public static readonly DomainError BaseCurrencyNotFound =
        DomainError.NotFound(
            code: "Transaction.BaseCurrencyNotFound",
            message: "Базовая валюта не найдена.");

    public static readonly DomainError TransactionNotFound =
        DomainError.NotFound(
            code: "Transaction.TransactionNotFound",
            message: "Транзакция не найдена.");
}