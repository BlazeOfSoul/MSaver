namespace MSaver.Application.Features.Categories.Update;

public sealed record UpdateCategoryRequest(
    Guid Id,
    string Name,
    string Color,
    string Type);