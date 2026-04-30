using MSaver.Application.Common.Specifications;

namespace MSaver.Application.Features.Tags.Specifications;

public sealed class TagByIdSpecification : BaseSpecification<Tag>
{
    public TagByIdSpecification(Guid id)
    {
        AddCriteria(x => x.Id == id);
        AddInclude(x => x.TagCategories);
    }
}