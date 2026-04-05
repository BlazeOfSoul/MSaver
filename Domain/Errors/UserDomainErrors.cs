namespace MSaver.Domain.Errors;

public static class UserDomainErrors
{
    public static readonly DomainError NameRequired =
        DomainError.Validation(
            code: "User.NameRequired",
            message: "Имя пользователя обязательно.",
            field: "name");

    public static readonly DomainError EmailRequired =
        DomainError.Validation(
            code: "User.EmailRequired",
            message: "Email обязателен.",
            field: "email");

    public static readonly DomainError PasswordHashRequired =
        DomainError.Validation(
            code: "User.PasswordHashRequired",
            message: "Хеш пароля обязателен.",
            field: "passwordHash");

    public static readonly DomainError IdNotFound =
        DomainError.Failure(
            code: "User.IdNotFound",
            message: "Не удалось определить идентификатор пользователя.");

    public static readonly DomainError UserIdRequired =
        DomainError.Validation(
            code: "User.UserIdRequired",
            message: "Идентификатор пользователя обязателен.",
            field: "userId");

    public static readonly DomainError UserNotFound =
        DomainError.NotFound(
            code: "User.NotFound",
            message: "Пользователь не найден.");

    public static readonly DomainError EmailAlreadyExists =
        DomainError.Conflict(
            code: "User.EmailAlreadyExists",
            message: "Пользователь с таким email уже существует.",
            field: "email");

    public static readonly DomainError NameAlreadyExists =
        DomainError.Conflict(
            code: "User.NameAlreadyExists",
            message: "Пользователь с таким именем уже существует.",
            field: "name");

    public static readonly DomainError InvalidCredentials =
        DomainError.Failure(
            code: "User.InvalidCredentials",
            message: "Неверный email или пароль.");
}