using MSaver.Domain.Enums;

namespace MSaver.Application.Features.Categories.Common;

public static class CategoryTypeHelper
{
    public static bool IsValid(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            || TryParse(value, out _);
    }

    public static bool TryParse(string? value, out CategoryType type)
    {
        if (Enum.TryParse(value, true, out type) && Enum.IsDefined(type))
            return true;

        type = default;
        return false;
    }

    public static string[] GetAllowedNames()
    {
        return Enum.GetNames<CategoryType>();
    }
}
