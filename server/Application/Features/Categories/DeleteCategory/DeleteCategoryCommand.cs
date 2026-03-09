namespace server.Application.Features.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid CategoryId, Guid UserId);