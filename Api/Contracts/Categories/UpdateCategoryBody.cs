namespace MSaver.Api.Contracts.Categories;

public sealed record UpdateCategoryBody(
    string Name,
    string Color,
    string Type);