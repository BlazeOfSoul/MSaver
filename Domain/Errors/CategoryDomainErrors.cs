using MSaver.Domain.Common;

namespace MSaver.Domain.Errors;

public static class CategoryDomainErrors
{
    public static readonly DomainError NameRequired =
        DomainError.Validation(
            code: "Category.NameRequired",
            message: "Название категории обязательно.",
            field: "name");

    public static readonly DomainError ColorRequired =
        DomainError.Validation(
            code: "Category.ColorRequired",
            message: "Цвет категории обязателен.",
            field: "color");

    public static readonly DomainError UserIdRequired =
        DomainError.Validation(
            code: "Category.UserIdRequired",
            message: "Идентификатор пользователя обязателен.",
            field: "userId");

    public static readonly DomainError NotFound =
        DomainError.NotFound(
            code: "Category.NotFound",
            message: "Категория не найдена.");

    public static readonly DomainError AccessDenied =
        DomainError.Failure(
            code: "Category.AccessDenied",
            message: "Недостаточно прав для доступа к категории.");
}