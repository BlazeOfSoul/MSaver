using MediatR;

namespace server.Features.Categories.GetCategories;

public record GetCategoriesQuery(Guid UserId) : IRequest<List<CategoryResponse>>;
