using MSaver.Domain.Common;

namespace MSaver.Domain.Errors;

public static class AuthDomainErrors
{
    public static readonly DomainError InvalidEmail =
        DomainError.Validation(
            code: "Auth.InvalidEmail",
            message: "Неверный email или пароль.",
            field: "email");

    public static readonly DomainError InvalidPassword =
        DomainError.Validation(
            code: "Auth.InvalidPassword",
            message: "Неверный email или пароль.",
            field: "password");

    public static readonly DomainError RepeatedEmail =
        DomainError.Conflict(
            code: "Auth.RepeatedEmail",
            message: "Пользователь с таким email уже существует.",
            field: "email");

    public static readonly DomainError RefreshTokenInvalid =
        DomainError.Validation(
            code: "Auth.RefreshTokenInvalid",
            message: "Недействительный refresh токен.",
            field: "refreshToken");

    public static readonly DomainError RefreshTokenExpired =
        DomainError.Validation(
            code: "Auth.RefreshTokenExpired",
            message: "Срок действия refresh токена истёк.",
            field: "refreshToken");
}