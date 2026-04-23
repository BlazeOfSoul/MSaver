using MSaver.Api.Contracts.Categories;
using MSaver.Application.Common.Models;
using MSaver.Application.Common.Validation;
using MSaver.Application.Features.Categories.Common;

namespace MSaver.Application.Features.Categories.Get;

public sealed class GetCategoriesRequestValidator : AbstractValidator<GetCategoriesRequest>
{
    public GetCategoriesRequestValidator()
    {
        RuleFor(x => x.Page)
            .ValidPage();

        RuleFor(x => x.Size)
            .ValidPageSize(ListQueryDefaults.MaxPageSize);

        RuleFor(x => x.Search)
            .MaximumLength(50)
            .WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.SortBy)
            .ValidSortBy(CategorySortFields.All);

        RuleFor(x => x.SortDirection)
            .ValidSortDirection();

        RuleFor(x => x.Type)
            .ValidCategoryType();
    }
}