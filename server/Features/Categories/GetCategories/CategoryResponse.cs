using server.Models.Enums;

namespace server.Features.Categories.GetCategories;

public record CategoryResponse(Guid Id, string Name, CategoryType Type, string Color);