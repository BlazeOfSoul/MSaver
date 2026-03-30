using server.Application.Common.Results;
using server.Application.Features.Categories.CreateCategory;
using server.Application.Features.Categories.DeleteCategory;
using server.Application.Features.Categories.GetCategories;
using server.Application.Features.Categories.UpdateCategory;

namespace server.Application.Abstractions.Services;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<CategoryResponse>>> GetCategoriesAsync(
        GetCategoriesRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CreateCategoryResponse>> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateCategoryAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteCategoryAsync(
        DeleteCategoryRequest request,
        CancellationToken cancellationToken = default);
}
