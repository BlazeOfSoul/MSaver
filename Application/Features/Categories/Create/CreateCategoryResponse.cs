using MSaver.Domain.Enums;

namespace MSaver.Application.Features.Categories.Create;

public sealed class CreateCategoryResponse(Guid id, string name, CategoryType type, string color)
{
    public Guid Id { get; set; } = id;

    public string Name { get; set; } = name;

    public CategoryType Type { get; set; } = type;

    public string Color { get; set; } = color;
}