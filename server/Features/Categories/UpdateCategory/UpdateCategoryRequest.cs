using server.Models.Enums;

namespace server.Features.Categories.UpdateCategory;

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#ffffff";
    
    public CategoryType Type { get; set; }
}