using MSaver.Domain.Common;

namespace MSaver.Domain.Errors;

public static class BalanceDomainErrors
{
    public static readonly DomainError UserIdRequired =
        DomainError.Validation(
            code: "Balance.UserIdRequired",
            message: "Идентификатор пользователя обязателен.",
            field: "userId");

    public static readonly DomainError InvalidYear =
        DomainError.Validation(
            code: "Balance.InvalidYear",
            message: "Год баланса указан неверно.",
            field: "year");

    public static readonly DomainError InvalidMonth =
        DomainError.Validation(
            code: "Balance.InvalidMonth",
            message: "Месяц баланса указан неверно.",
            field: "month");

    public static readonly DomainError NegativeAmount =
        DomainError.Validation(
            code: "Balance.NegativeAmount",
            message: "Сумма не может быть отрицательной.",
            field: "amount");

    public static readonly DomainError NotFound =
        DomainError.NotFound(
            code: "Balance.NotFound",
            message: "Текущий баланс не найден.");
}