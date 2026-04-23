namespace MSaver.Application.Common.Validation;

public static class CommonValidationExtensions
{
    public static IRuleBuilderOptions<T, int> ValidPage<T>(
        this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage(ValidationMessages.MustBePositive);
    }

    public static IRuleBuilderOptions<T, int> ValidPageSize<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        int maxSize = ListQueryDefaults.MaxPageSize)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage(ValidationMessages.MustBePositive)
            .LessThanOrEqualTo(maxSize)
            .WithMessage(ListValidationMessages.MaxPageSize.Replace("{MaxSize}", maxSize.ToString()));
    }

    public static IRuleBuilderOptions<T, string?> ValidSortDirection<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(x =>
                string.IsNullOrWhiteSpace(x) ||
                string.Equals(x, ListQueryDefaults.SortAscending, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x, ListQueryDefaults.SortDescending, StringComparison.OrdinalIgnoreCase))
            .WithMessage(ListValidationMessages.InvalidSortDirection);
    }

    public static IRuleBuilderOptions<T, string?> ValidSortBy<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        params string[] allowedFields)
    {
        return ruleBuilder
            .Must(x =>
                string.IsNullOrWhiteSpace(x) ||
                allowedFields.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage(ListValidationMessages.InvalidSortField);
    }

    public static IRuleBuilderOptions<T, TModel> ValidDateRange<T, TModel>(
        this IRuleBuilder<T, TModel> ruleBuilder,
        Func<TModel, DateTime?> fromSelector,
        Func<TModel, DateTime?> toSelector)
    {
        return ruleBuilder
            .Must(model =>
            {
                var from = fromSelector(model);
                var to = toSelector(model);

                return !from.HasValue || !to.HasValue || from.Value <= to.Value;
            })
            .WithMessage(ListValidationMessages.InvalidDateRange);
    }
}