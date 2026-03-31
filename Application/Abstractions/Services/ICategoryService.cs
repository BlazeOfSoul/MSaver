using MSaver.Application.Common.Results;
using MSaver.Application.Features.Categories.CreateCategory;
using MSaver.Application.Features.Categories.DeleteCategory;
using MSaver.Application.Features.Categories.GetCategories;
using MSaver.Application.Features.Categories.UpdateCategory;

namespace MSaver.Application.Abstractions.Services;

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
