using server.Application.Features.Categories.CreateCategory;
using server.Application.Features.Categories.DeleteCategory;
using server.Application.Features.Categories.GetCategories;
using server.Application.Features.Categories.UpdateCategory;

namespace server.Application.Services.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(
        GetCategoriesRequest request,
        CancellationToken cancellationToken = default);

    Task<CreateCategoryResponse> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateCategoryAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteCategoryAsync(
        DeleteCategoryRequest request,
        CancellationToken cancellationToken = default);
}
