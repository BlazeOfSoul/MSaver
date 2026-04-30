namespace MSaver.Application.Features.Categories.Common;

public static class CategoryValidationExtensions
{
    public static IRuleBuilderOptions<T, string?> ValidCategoryType<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(CategoryTypeHelper.IsValid)
            .WithMessage(CategoryValidationMessages.InvalidCategoryType);
    }
}