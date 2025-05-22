using MediatR;

namespace server.Features.Categories.DeleteCategory;

public record DeleteCategoryCommand(Guid CategoryId, Guid UserId) : IRequest<bool>;