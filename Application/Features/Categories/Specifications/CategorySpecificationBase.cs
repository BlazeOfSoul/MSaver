using MSaver.Application.Common.Specifications;
using MSaver.Application.Features.Categories.Get;

namespace MSaver.Application.Features.Categories.Specifications;

public abstract class CategorySpecificationBase : BaseSpecification<Category>
{
    protected void ApplyBaseFilters(CategoryListQuery query)
    {
        AddCriteriaInternal(x => x.UserId == query.UserId);
        AddCriteriaInternal(x => !x.IsDeleted);

        if (query.Type.HasValue)
            AddCriteriaInternal(x => x.Type == query.Type.Value);

        this.ApplyContainsIfNotEmpty(query.Search, search => x => x.Name.Contains(search));
    }

    protected void ApplySorting(string? sortBy, string? sortDirection)
    {
        var normalizedSortBy = ListQueryHelper.NormalizeSortBy(sortBy, CategorySortFields.Name);
        var normalizedSortDirection = ListQueryHelper.NormalizeSortDirection(sortDirection);

        switch (normalizedSortBy)
        {
            case var field when field.Equals(CategorySortFields.Type, StringComparison.OrdinalIgnoreCase):
                this.ApplyOrderWithDirection(normalizedSortDirection, x => x.Type);
                AddThenByInternal(x => x.Name);
                AddThenByInternal(x => x.Id);
                break;

            default:
                this.ApplyOrderWithDirection(normalizedSortDirection, x => x.Name);
                if (normalizedSortDirection == ListQueryDefaults.SortDescending)
                    this.AddTieBreakerByIdDescending(x => x.Id);
                else
                    this.AddTieBreakerByIdAscending(x => x.Id);
                break;
        }
    }
}
