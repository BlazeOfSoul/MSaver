using MSaver.Domain.Enums;

namespace MSaver.Application.Features.Categories.Common;

public static class CategoryTypeHelper
{
    public static bool IsValid(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            || Enum.TryParse<CategoryType>(value, true, out _);
    }

    public static bool TryParse(string? value, out CategoryType type)
    {
        return Enum.TryParse(value, true, out type);
    }

    public static string[] GetAllowedNames()
    {
        return Enum.GetNames<CategoryType>();
    }
}