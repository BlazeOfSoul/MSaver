using MediatR;
using server.Models.Enums;

namespace server.Features.Categories.UpdateCategory;

public record UpdateCategoryCommand(Guid CategoryId, Guid UserId, string Name, string Color, CategoryType Type) : IRequest<bool>;