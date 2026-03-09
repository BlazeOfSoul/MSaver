using server.Application.Features.Categories.CreateCategory;
using server.Application.Features.Categories.DeleteCategory;
using server.Application.Features.Categories.GetCategories;
using server.Application.Features.Categories.UpdateCategory;

namespace server.Application.Services.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(GetCategoriesQuery query, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default);
    Task<bool> UpdateCategoryAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(DeleteCategoryCommand command, CancellationToken cancellationToken = default);
}
