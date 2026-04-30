using System.Linq.Expressions;

namespace MSaver.Application.Common.Specifications;

public static class SpecificationExtensions
{
    public static void ApplyPage<T>(
        this BaseSpecification<T> specification,
        int page,
        int size)
    {
        specification.ApplyPageInternal(page, size);
    }

    public static void ApplyContainsIfNotEmpty<T>(
        this BaseSpecification<T> specification,
        string? value,
        Func<string, Expression<Func<T, bool>>> predicateFactory)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var normalized = value.Trim();
        specification.AddCriteriaInternal(predicateFactory(normalized));
    }

    public static void ApplyOrderWithDirection<T>(
        this BaseSpecification<T> specification,
        string direction,
        Expression<Func<T, object>> ascending,
        Expression<Func<T, object>> descending)
    {
        if (string.Equals(direction, ListQueryDefaults.SortDescending, StringComparison.OrdinalIgnoreCase))
        {
            specification.ApplyOrderByDescendingInternal(descending);
            return;
        }

        specification.ApplyOrderByInternal(ascending);
    }

    public static void ApplyOrderWithDirection<T>(
        this BaseSpecification<T> specification,
        string direction,
        Expression<Func<T, object>> fieldSelector)
    {
        if (string.Equals(direction, ListQueryDefaults.SortDescending, StringComparison.OrdinalIgnoreCase))
        {
            specification.ApplyOrderByDescendingInternal(fieldSelector);
            return;
        }

        specification.ApplyOrderByInternal(fieldSelector);
    }

    public static void AddTieBreakerByIdAscending<T>(
        this BaseSpecification<T> specification,
        Expression<Func<T, object>> idSelector)
    {
        specification.AddThenByInternal(idSelector);
    }

    public static void AddTieBreakerByIdDescending<T>(
        this BaseSpecification<T> specification,
        Expression<Func<T, object>> idSelector)
    {
        specification.AddThenByDescendingInternal(idSelector);
    }

    public static void AddIncludePath<T>(
        this BaseSpecification<T> specification,
        Expression<Func<T, object>> includeExpression)
    {
        specification.AddIncludeInternal(includeExpression);
    }
}