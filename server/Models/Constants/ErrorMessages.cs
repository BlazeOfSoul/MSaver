namespace server.Models.Constants;
public static class ErrorMessages
{
    public static class Auth
    {
        public const string InvalidEmail = "Неверный Email.";
        public const string InvalidPassword = "Неверный пароль.";
        public const string RepeatedEmail = "Пользователь с таким email уже существует.";
    }

    public static class Categories
    {
        public const string NotFound = "Категория не найдена.";
    }

    public static class User
    {
        public const string IdNotFound = "User ID не найден в токене.";

    }

    public static class ExchangeRate
    {
        public const string NbrbRateNotFound = "Не удалось получить курсы валют от NBRB.";
        public const string CoinGeckoNotFound = "Не удалось получить курсы криптовалют от CoinGecko.";
    }
    
    public static class Balance
    {
        public const string NotFound = "Баланс не найден";

    }
}
