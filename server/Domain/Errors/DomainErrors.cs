namespace server.Domain.Errors;

public static class DomainErrors
{
    public static class User
    {
        public const string UsernameRequired = "Имя пользователя обязательно.";
        public const string EmailRequired = "Email обязателен.";
        public const string PasswordHashRequired = "Хеш пароля обязателен.";
    }

    public static class Category
    {
        public const string NameRequired = "Название категории обязательно.";
        public const string ColorRequired = "Цвет категории обязателен.";
        public const string UserIdRequired = "Идентификатор пользователя обязателен.";
    }

    public static class Balance
    {
        public const string UserIdRequired = "Идентификатор пользователя обязателен.";
        public const string InvalidYear = "Год баланса указан неверно.";
        public const string InvalidMonth = "Месяц баланса указан неверно.";
        public const string NegativeAmount = "Сумма не может быть отрицательной.";
    }

    public static class Transaction
    {
        public const string UserIdRequired = "Идентификатор пользователя обязателен.";
        public const string CategoryIdRequired = "Идентификатор категории обязателен.";
        public const string AmountMustBePositive = "Сумма транзакции должна быть больше нуля.";
    }
}
