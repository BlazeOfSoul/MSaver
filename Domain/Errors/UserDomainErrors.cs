using MSaver.Domain.Common;

namespace MSaver.Domain.Errors;

public static class UserDomainErrors
{
    public static readonly DomainError UsernameRequired =
        DomainError.Validation(
            code: "User.UsernameRequired",
            message: "Имя пользователя обязательно.",
            field: "username");

    public static readonly DomainError EmailRequired =
        DomainError.Validation(
            code: "User.EmailRequired",
            message: "Email обязателен.",
            field: "email");

    public static readonly DomainError PasswordHashRequired =
        DomainError.Validation(
            code: "User.PasswordHashRequired",
            message: "Хеш пароля обязателен.",
            field: "password");

    public static readonly DomainError IdNotFound =
        DomainError.Failure(
            code: "User.IdNotFound",
            message: "Не удалось определить идентификатор пользователя.");

    public static readonly DomainError UserIdRequired =
        DomainError.Validation(
            code: "User.UserIdRequired",
            message: "Идентификатор пользователя обязателен.",
            field: "userId");
}