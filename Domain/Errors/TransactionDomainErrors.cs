namespace MSaver.Domain.Errors;

public static class TransactionDomainErrors
{
    public static readonly DomainError UserIdRequired =
        DomainError.Validation(
            code: "Transaction.UserIdRequired",
            message: "Идентификатор пользователя обязателен.",
            field: "userId");

    public static readonly DomainError TransactionIdRequired =
        DomainError.Validation(
            code: "Transaction.IdRequired",
            message: "Идентификатор транзакции обязателен.",
            field: "id");

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

    public static readonly DomainError AmountPrecisionInvalid =
        DomainError.Validation(
            code: "Transaction.AmountPrecisionInvalid",
            message: "Сумма содержит больше знаков после запятой, чем поддерживает валюта счёта.",
            field: "amount");

    public static readonly DomainError InvalidDate =
        DomainError.Validation(
            code: "Transaction.InvalidDate",
            message: "Некорректная дата транзакции.",
            field: "date");

    public static readonly DomainError TransferIdRequired =
        DomainError.Validation(
            code: "Transaction.TransferIdRequired",
            message: "Идентификатор перевода обязателен.",
            field: "transferId");

    public static readonly DomainError TransferFromAccountIdRequired =
        DomainError.Validation(
            code: "Transaction.TransferFromAccountIdRequired",
            message: "Идентификатор счёта списания обязателен.",
            field: "fromAccountId");

    public static readonly DomainError TransferToAccountIdRequired =
        DomainError.Validation(
            code: "Transaction.TransferToAccountIdRequired",
            message: "Идентификатор счёта зачисления обязателен.",
            field: "toAccountId");

    public static readonly DomainError TransferCategoryRequiresTransferEndpoint =
        DomainError.Validation(
            code: "Transaction.TransferCategoryRequiresTransferEndpoint",
            message: "Категории переводов можно использовать только через endpoint переводов.",
            field: "categoryId");

    public static readonly DomainError TransferTransactionRequiresTransferEndpoint =
        DomainError.Validation(
            code: "Transaction.TransferTransactionRequiresTransferEndpoint",
            message: "Транзакции переводов можно изменять только через endpoint переводов.",
            field: "transferId");

    public static readonly DomainError TransferRateMustBePositive =
        DomainError.Validation(
            code: "Transaction.TransferRateMustBePositive",
            message: "Курс перевода должен быть больше нуля.",
            field: "rate");

    public static readonly DomainError TransferRateMustBeOneForSameCurrency =
        DomainError.Validation(
            code: "Transaction.TransferRateMustBeOneForSameCurrency",
            message: "Курс перевода должен быть равен 1, если счета в одной валюте.",
            field: "rate");

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

    public static readonly DomainError TransferDepositAmountMustBeGreaterThanZero =
        DomainError.Validation(
            code: "Transaction.TransferDepositAmountMustBeGreaterThanZero",
            message: "Сумма перевода после округления в валюте перевода должна быть больше нуля.",
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
    public static readonly DomainError TransferNotFound =
        DomainError.NotFound(
            code: "Transaction.TransferNotFound",
            message: "Перевод не найден.");

    public static readonly DomainError TransferPairInvalid =
        DomainError.Conflict(
            code: "Transaction.TransferPairInvalid",
            message: "Перевод должен состоять ровно из одной транзакции списания и одной транзакции зачисления.");
}
