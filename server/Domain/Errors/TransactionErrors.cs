namespace server.Domain.Errors;

public static class TransactionErrors
{
    public const string UserIdRequired = "Идентификатор пользователя обязателен.";
    public const string CategoryIdRequired = "Идентификатор категории обязателен.";
    public const string AmountMustBePositive = "Сумма транзакции должна быть больше нуля.";
}