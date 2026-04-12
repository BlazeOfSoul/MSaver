using MSaver.Domain.Enums;

namespace MSaver.Application.Features.Categories.Get;

public sealed class CategoryItemResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public CategoryType Type { get; init; }

    public string Color { get; init; } = string.Empty;
}