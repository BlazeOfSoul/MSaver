using server.Domain.Enums;

namespace server.Application.Features.Categories.GetCategories;

public sealed record CategoryResponse(Guid Id, string Name, CategoryType Type, string Color);