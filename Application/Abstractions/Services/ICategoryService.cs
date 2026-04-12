using MSaver.Application.Features.Categories.Create;
using MSaver.Application.Features.Categories.Get;
using MSaver.Application.Features.Categories.Update;

namespace MSaver.Application.Abstractions.Services;

public interface ICategoryService
{
    Task<Result<Guid>> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> UpdateAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<GetCategoriesResponse>> GetAsync(
        CancellationToken cancellationToken = default);
}