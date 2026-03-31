using MSaver.Domain.Common;

namespace MSaver.Domain.Errors;

public static class UserDomainErrors
{
    public static readonly DomainError UsernameRequired =
        new(
            DomainErrorType.Validation,
            "User.UsernameRequired",
            "Имя пользователя обязательно.",
            "username");

    public static readonly DomainError EmailRequired =
        new(
            DomainErrorType.Validation,
            "User.EmailRequired",
            "Email обязателен.",
            "email");

    public static readonly DomainError PasswordHashRequired =
        new(
            DomainErrorType.Validation,
            "User.PasswordHashRequired",
            "Хеш пароля обязателен.",
            "password");

    public static readonly DomainError IdNotFound =
        new(
            DomainErrorType.Failure,
            "User.IdNotFound",
            "Не удалось определить идентификатор пользователя.",
            null);

    public static readonly DomainError UserIdRequired =
        new(
            DomainErrorType.Validation,
            "User.UserIdRequired",
            "Идентификатор пользователя обязателен.",
            "userId");
}
