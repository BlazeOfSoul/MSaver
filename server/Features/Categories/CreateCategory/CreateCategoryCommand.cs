using MediatR;
using server.Models.Enums;

namespace server.Features.Categories.CreateCategory;

public class CreateCategoryCommand : IRequest<CategoryDto>
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public CategoryType Type { get; set; }
    public string Color { get; set; } = null!;
}