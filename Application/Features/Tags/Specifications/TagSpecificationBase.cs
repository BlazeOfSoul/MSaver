using MSaver.Application.Common.Specifications;
using MSaver.Application.Features.Tags.Get;

namespace MSaver.Application.Features.Tags.Specifications;

public abstract class TagSpecificationBase : BaseSpecification<Tag>
{
    protected void ApplyBaseFilters(TagListQuery query)
    {
        AddCriteriaInternal(x => x.UserId == query.UserId);
        AddCriteriaInternal(x => !x.IsDeleted);
        this.ApplyContainsIfNotEmpty(query.Search, search => x => x.Name.Contains(search));
    }

    protected void ApplySorting(string? sortBy, string? sortDirection)
    {
        var normalizedSortBy = ListQueryHelper.NormalizeSortBy(sortBy, TagSortFields.Name);
        var normalizedSortDirection = ListQueryHelper.NormalizeSortDirection(sortDirection);

        switch (normalizedSortBy)
        {
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