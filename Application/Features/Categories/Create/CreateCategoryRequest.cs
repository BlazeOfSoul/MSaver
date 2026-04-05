using MSaver.Domain.Enums;

namespace MSaver.Application.Features.Categories.Create;

public sealed class CreateCategoryRequest
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public CategoryType Type { get; set; }

    public string Color { get; set; } = null!;
}