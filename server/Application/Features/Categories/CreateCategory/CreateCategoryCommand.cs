using server.Domain.Enums;

namespace server.Application.Features.Categories.CreateCategory;

public sealed class CreateCategoryCommand
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public CategoryType Type { get; set; }
    public string Color { get; set; } = null!;
}