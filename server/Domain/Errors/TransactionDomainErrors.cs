using server.Domain.Common;

namespace server.Domain.Errors;

public static class TransactionDomainErrors
{
    public static readonly DomainError UserIdRequired =
        new(
            DomainErrorType.Validation,
            "Transaction.UserIdRequired",
            "Идентификатор пользователя обязателен.",
            "userId");

    public static readonly DomainError CategoryIdRequired =
        new(
            DomainErrorType.Validation,
            "Transaction.CategoryIdRequired",
            "Идентификатор категории обязателен.",
            "categoryId");

    public static readonly DomainError AmountMustBePositive =
        new(
            DomainErrorType.Validation,
            "Transaction.AmountMustBePositive",
            "Сумма транзакции должна быть больше нуля.",
            "amount");

    public static readonly DomainError CategoryNotFound =
        new(
            DomainErrorType.NotFound,
            "Transaction.CategoryNotFound",
            "Категория не найдена.",
            null);
}
