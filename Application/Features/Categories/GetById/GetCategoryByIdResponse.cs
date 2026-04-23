using MSaver.Domain.Enums;

namespace MSaver.Application.Features.Categories.GetById;

public sealed class GetCategoryByIdResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public CategoryType Type { get; init; }

    public string Color { get; init; } = string.Empty;

    public bool IsDeleted { get; init; }

    public bool IsSystem { get; init; }
}