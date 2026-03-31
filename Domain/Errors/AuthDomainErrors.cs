using MSaver.Domain.Common;

namespace MSaver.Domain.Errors;

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

    public static readonly DomainError RefreshTokenInvalid =
        new(
            DomainErrorType.Validation,
            "Auth.RefreshTokenInvalid",
            "Недействительный refresh токен.",
            "refreshToken");

    public static readonly DomainError RefreshTokenExpired =
        new(
            DomainErrorType.Validation,
            "Auth.RefreshTokenExpired",
            "Срок действия refresh токена истёк.",
            "refreshToken");
}
