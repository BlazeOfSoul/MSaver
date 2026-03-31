using MSaver.Domain.Common;

namespace MSaver.Domain.Errors;

public static class TransactionDomainErrors
{
    public static readonly DomainError UserIdRequired =
        DomainError.Validation(
            code: "Transaction.UserIdRequired",
            message: "Идентификатор пользователя обязателен.",
            field: "userId");

    public static readonly DomainError CategoryIdRequired =
        DomainError.Validation(
            code: "Transaction.CategoryIdRequired",
            message: "Идентификатор категории обязателен.",
            field: "categoryId");

    public static readonly DomainError AmountMustBePositive =
        DomainError.Validation(
            code: "Transaction.AmountMustBePositive",
            message: "Сумма транзакции должна быть больше нуля.",
            field: "amount");

    public static readonly DomainError CategoryNotFound =
        DomainError.NotFound(
            code: "Transaction.CategoryNotFound",
            message: "Категория не найдена.");
}