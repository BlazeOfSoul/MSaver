using MSaver.Domain.Enums;

namespace MSaver.Application.Features.Categories.GetCategories;

public sealed record CategoryResponse(Guid Id, string Name, CategoryType Type, string Color);