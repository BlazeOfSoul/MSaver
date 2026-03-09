using server.Domain.Enums;

namespace server.Application.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#ffffff";
    public CategoryType Type { get; set; }
}