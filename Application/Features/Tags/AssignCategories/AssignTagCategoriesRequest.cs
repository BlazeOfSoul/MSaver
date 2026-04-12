namespace MSaver.Application.Features.Tags.AssignCategories;

public sealed class AssignTagCategoriesRequest
{
    public Guid TagId { get; set; }
    public IReadOnlyCollection<Guid> CategoryIds { get; set; } = [];
}