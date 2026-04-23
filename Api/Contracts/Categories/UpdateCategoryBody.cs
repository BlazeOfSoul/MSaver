using MSaver.Domain.Enums;

namespace MSaver.Api.Contracts.Categories;

public sealed record UpdateCategoryBody(
    string Name,
    string Color,
    CategoryType Type);