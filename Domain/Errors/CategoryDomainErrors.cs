namespace MSaver.Domain.Errors;

public static class CategoryDomainErrors
{
    public static readonly DomainError UserIdRequired =
        DomainError.Validation(
            code: "Category.UserIdRequired",
            message: "Идентификатор пользователя обязателен.",
            field: "userId");

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

    public static readonly DomainError InvalidCategoryType =
        DomainError.Validation(
            code: "Category.InvalidType",
            message: "Некорректный тип категории.",
            field: "type");

    public static readonly DomainError CategoryNotFound =
        DomainError.NotFound(
            code: "Category.NotFound",
            message: "Категория не найдена.");

    public static readonly DomainError AccessDenied =
        DomainError.Failure(
            code: "Category.AccessDenied",
            message: "Недостаточно прав для доступа к категории.");

    public static readonly DomainError CategoryDeleted =
        DomainError.Validation(
            code: "Category.Deleted",
            message: "Категория удалена.");

    public static readonly DomainError CategoryAlreadyDeleted =
        DomainError.Validation(
            code: "Category.AlreadyDeleted",
            message: "Категория уже удалена.");

    public static readonly DomainError CategoryAlreadyActive =
        DomainError.Validation(
            code: "Category.AlreadyActive",
            message: "Категория уже восстановлена.");

    public static readonly DomainError NameAlreadyExists =
        DomainError.Conflict(
            code: "Category.NameAlreadyExists",
            message: "Категория с таким названием уже существует.",
            field: "name");

    public static readonly DomainError ParentCategoryNotFound =
        DomainError.NotFound(
            code: "Category.ParentNotFound",
            message: "Родительская категория не найдена.");

    public static readonly DomainError ParentCategoryAccessDenied =
        DomainError.Failure(
            code: "Category.ParentAccessDenied",
            message: "Недостаточно прав для доступа к родительской категории.");

    public static readonly DomainError ParentCategoryCannotBeSame =
        DomainError.Validation(
            code: "Category.ParentCannotBeSame",
            message: "Категория не может быть родительской сама для себя.",
            field: "parentId");

    public static readonly DomainError DeletedCategoryCannotBeParent =
        DomainError.Validation(
            code: "Category.DeletedCategoryCannotBeParent",
            message: "Удалённая категория не может быть родительской.",
            field: "parentId");
}