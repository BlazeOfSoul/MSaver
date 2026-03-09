using server.Domain.Enums;

namespace server.Application.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid CategoryId,
    Guid UserId,
    string Name,
    string Color,
    CategoryType Type);