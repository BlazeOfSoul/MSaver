using server.Domain.Common;

namespace server.Domain.Errors;

public static class CategoryDomainErrors
{
    public static readonly DomainError NameRequired =
        new(
            DomainErrorType.Validation,
            "Category.NameRequired",
            "Название категории обязательно.",
            "name");

    public static readonly DomainError ColorRequired =
        new(
            DomainErrorType.Validation,
            "Category.ColorRequired",
            "Цвет категории обязателен.",
            "color");

    public static readonly DomainError UserIdRequired =
        new(
            DomainErrorType.Validation,
            "Category.UserIdRequired",
            "Идентификатор пользователя обязателен.",
            "userId");

    public static readonly DomainError NotFound =
        new(
            DomainErrorType.NotFound,
            "Category.NotFound",
            "Категория не найдена.",
            null);

    public static readonly DomainError AccessDenied =
        new(
            DomainErrorType.Failure,
            "Category.AccessDenied",
            "Недостаточно прав для доступа к категории.",
            null);
}
