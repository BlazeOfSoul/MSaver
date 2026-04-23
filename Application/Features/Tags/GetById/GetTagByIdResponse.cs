namespace MSaver.Application.Features.Tags.GetById;

public sealed class GetTagByIdResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Color { get; init; }

    public bool IsDeleted { get; init; }

    public IReadOnlyCollection<TagCategoryResponse> Categories { get; init; } = [];
}