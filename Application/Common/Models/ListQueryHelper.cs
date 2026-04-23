namespace MSaver.Application.Common.Models;

public static class ListQueryHelper
{
    public static string NormalizeSortDirection(string? value)
    {
        return string.Equals(
            value,
            ListQueryDefaults.SortAscending,
            StringComparison.OrdinalIgnoreCase)
                ? ListQueryDefaults.SortAscending
                : ListQueryDefaults.SortDescending;
    }

    public static string NormalizeSortBy(
        string? value,
        string defaultSortBy)
    {
        return string.IsNullOrWhiteSpace(value)
            ? defaultSortBy
            : value.Trim();
    }

    public static string? NormalizeSearch(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    public static string? NormalizeUpperInvariant(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToUpperInvariant();
    }
}