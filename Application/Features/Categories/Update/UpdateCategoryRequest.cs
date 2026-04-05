using MSaver.Domain.Enums;

namespace MSaver.Application.Features.Categories.Update;

public sealed record UpdateCategoryRequest(
    Guid Id,
    Guid UserId,
    string Name,
    string Color,
    CategoryType Type);
