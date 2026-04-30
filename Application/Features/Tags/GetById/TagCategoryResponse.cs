namespace MSaver.Application.Features.Tags.GetById;

public sealed class TagCategoryResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Color { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public bool IsDeleted { get; init; }
}