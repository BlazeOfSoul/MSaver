using MSaver.Application.Features.Categories.Get;

namespace MSaver.Application.Features.Categories.Specifications;

public sealed class CategoriesListSpecification : CategorySpecificationBase
{
    public CategoriesListSpecification(CategoryListQuery query)
    {
        ApplyBaseFilters(query);
        ApplySorting(query.SortBy, query.SortDirection);
        ApplyPage(query.Page, query.Size);
    }
}