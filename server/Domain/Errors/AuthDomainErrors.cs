using server.Domain.Common;

namespace server.Domain.Errors;

public static class AuthDomainErrors
{
    public static readonly DomainError InvalidEmail =
        new(
            DomainErrorType.Validation,
            "Auth.InvalidEmail",
            "Неверный email или пароль.",
            "email");

    public static readonly DomainError InvalidPassword =
        new(
            DomainErrorType.Validation,
            "Auth.InvalidPassword",
            "Неверный email или пароль.",
            "password");

    public static readonly DomainError RepeatedEmail =
        new(
            DomainErrorType.Conflict,
            "Auth.RepeatedEmail",
            "Пользователь с таким email уже существует.",
            "email");
}
