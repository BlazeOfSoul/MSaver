using MSaver.Application.Features.Tags.Get;

namespace MSaver.Application.Features.Tags.Specifications;

public sealed class TagsCountSpecification : TagSpecificationBase
{
    public TagsCountSpecification(TagListQuery query)
    {
        ApplyBaseFilters(query);
    }
}