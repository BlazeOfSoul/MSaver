using server.Domain.Enums;

namespace server.Application.Features.Categories.CreateCategory;

public sealed class CreateCategoryResponse
{
    public CreateCategoryResponse(Guid id, string name, CategoryType type, string color)
    {
        Id = id;
        Name = name;
        Type = type;
        Color = color;
    }

    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public CategoryType Type { get; set; }
    public string Color { get; set; } = null!;
}
