using MSaver.Application.Features.Categories.Get;

namespace MSaver.Application.Features.Categories.Specifications;

public sealed class CategoriesCountSpecification : CategorySpecificationBase
{
    public CategoriesCountSpecification(CategoryListQuery query)
    {
        ApplyBaseFilters(query);
    }
}