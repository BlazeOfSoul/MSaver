namespace server.Domain.Errors;

public static class BalanceErrors
{
    public const string UserIdRequired = "Идентификатор пользователя обязателен.";
    public const string InvalidYear = "Год баланса указан неверно.";
    public const string InvalidMonth = "Месяц баланса указан неверно.";
    public const string NegativeAmount = "Сумма не может быть отрицательной.";
}