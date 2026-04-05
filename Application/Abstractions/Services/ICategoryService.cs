using MSaver.Application.Features.Categories.Create;
using MSaver.Application.Features.Categories.Delete;
using MSaver.Application.Features.Categories.Get;
using MSaver.Application.Features.Categories.Update;

namespace MSaver.Application.Abstractions.Services;

public interface ICategoryService
{
    Task<Result<CreateCategoryResponse>> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> UpdateAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> DeleteAsync(
        DeleteCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<GetCategoriesResponse>> GetAsync(
        GetCategoriesRequest request,
        CancellationToken cancellationToken = default);
}