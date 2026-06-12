using MSaver.Application.Common.Specifications;

namespace MSaver.Application.Features.Categories.Specifications;

public sealed class CategoryByIdSpecification : BaseSpecification<Category>
{
    public CategoryByIdSpecification(Guid id)
    {
        AddCriteria(x => x.Id == id);
        ApplyTracking();
    }
}
