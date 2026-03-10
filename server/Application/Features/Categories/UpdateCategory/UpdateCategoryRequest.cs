using server.Domain.Enums;

namespace server.Application.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryRequest(
    Guid CategoryId,
    Guid UserId,
    string Name,
    string Color,
    CategoryType Type);