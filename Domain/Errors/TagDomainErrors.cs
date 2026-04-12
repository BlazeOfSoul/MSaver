namespace MSaver.Domain.Errors;

public static class TagDomainErrors
{
    public static readonly DomainError UserIdRequired =
        DomainError.Validation(
            code: "Tag.UserIdRequired",
            message: "Идентификатор пользователя обязателен.",
            field: "userId");

    public static readonly DomainError NameRequired =
        DomainError.Validation(
            code: "Tag.NameRequired",
            message: "Название тега обязательно.",
            field: "name");

    public static readonly DomainError TagNotFound =
        DomainError.NotFound(
            code: "Tag.NotFound",
            message: "Тег не найден.");

    public static readonly DomainError AccessDenied =
        DomainError.Failure(
            code: "Tag.AccessDenied",
            message: "Недостаточно прав для доступа к тегу.");

    public static readonly DomainError TagDeleted =
        DomainError.Validation(
            code: "Tag.Deleted",
            message: "Тег удалён.");

    public static readonly DomainError TagAlreadyDeleted =
        DomainError.Validation(
            code: "Tag.AlreadyDeleted",
            message: "Тег уже удалён.");

    public static readonly DomainError TagAlreadyActive =
        DomainError.Validation(
            code: "Tag.AlreadyActive",
            message: "Тег уже восстановлен.");

    public static readonly DomainError NameAlreadyExists =
        DomainError.Conflict(
            code: "Tag.NameAlreadyExists",
            message: "Тег с таким названием уже существует.",
            field: "name");
}