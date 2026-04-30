namespace MSaver.Application.Common.Validation;

public static class ListValidationMessages
{
    public const string InvalidSortField = "Некорректное поле сортировки.";
    public const string InvalidSortDirection = "Некорректное направление сортировки.";
    public const string MaxPageSize = "Размер страницы не должен превышать {MaxSize} элементов.";
    public const string InvalidDateRange = "Дата начала периода не может быть позже даты окончания.";
}