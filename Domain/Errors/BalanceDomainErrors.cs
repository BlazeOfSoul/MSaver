using MSaver.Domain.Common;

namespace MSaver.Domain.Errors;

public static class BalanceDomainErrors
{
    public static readonly DomainError UserIdRequired =
        new(
            DomainErrorType.Validation,
            "Balance.UserIdRequired",
            "Идентификатор пользователя обязателен.",
            "userId");

    public static readonly DomainError InvalidYear =
        new(
            DomainErrorType.Validation,
            "Balance.InvalidYear",
            "Год баланса указан неверно.",
            "year");

    public static readonly DomainError InvalidMonth =
        new(
            DomainErrorType.Validation,
            "Balance.InvalidMonth",
            "Месяц баланса указан неверно.",
            "month");

    public static readonly DomainError NegativeAmount =
        new(
            DomainErrorType.Validation,
            "Balance.NegativeAmount",
            "Сумма не может быть отрицательной.",
            "amount");

    public static readonly DomainError NotFound =
        new(
            DomainErrorType.NotFound,
            "Balance.NotFound",
            "Текущий баланс не найден.",
            null);
}
