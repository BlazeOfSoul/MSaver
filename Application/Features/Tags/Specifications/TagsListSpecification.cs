using MSaver.Application.Features.Tags.Get;

namespace MSaver.Application.Features.Tags.Specifications;

public sealed class TagsListSpecification : TagSpecificationBase
{
    public TagsListSpecification(TagListQuery query)
    {
        ApplyBaseFilters(query);
        ApplySorting(query.SortBy, query.SortDirection);
        ApplyPage(query.Page, query.Size);
    }
}