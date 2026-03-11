using server.Application.Abstractions.Repositories;
using server.Application.Common.Results;
using server.Application.Features.Categories.CreateCategory;
using server.Application.Features.Categories.DeleteCategory;
using server.Application.Features.Categories.GetCategories;
using server.Application.Features.Categories.UpdateCategory;
using server.Application.Services.Interfaces;
using server.Domain.Common;
using server.Domain.Entities;
using server.Domain.Errors;

namespace server.Application.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<IReadOnlyList<CategoryResponse>>> GetCategoriesAsync(
        GetCategoriesRequest request,
        CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository
            .GetByUserIdAsync(request.UserId, cancellationToken);

        var response = categories
            .Where(c => !c.IsDeleted)
            .Select(c => new CategoryResponse(
                c.Id,
                c.Name,
                c.Type,
                c.Color))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<CategoryResponse>>.Success(response);
    }

    public async Task<Result<CreateCategoryResponse>> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var category = Category.Create(
                request.UserId,
                request.Name,
                request.Type,
                request.Color);

            await _categoryRepository.AddAsync(category, cancellationToken);

            var response = new CreateCategoryResponse(
                category.Id,
                category.Name,
                category.Type,
                category.Color);

            return Result<CreateCategoryResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            return Result<CreateCategoryResponse>.Failure(ex.Error);
        }
    }

    public async Task<Result> UpdateCategoryAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository
            .GetByIdAsync(request.Id, cancellationToken);

        if (category is null || category.IsDeleted)
            return Result.Failure(CategoryDomainErrors.NotFound);

        if (category.UserId != request.UserId)
            return Result.Failure(CategoryDomainErrors.AccessDenied);

        try
        {
            category.Update(request.Name, request.Color, request.Type);
            await _categoryRepository.UpdateAsync(category, cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Error);
        }
    }

    public async Task<Result> DeleteCategoryAsync(
        DeleteCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository
            .GetByIdAsync(request.Id, cancellationToken);

        if (category is null || category.IsDeleted)
            return Result.Failure(CategoryDomainErrors.NotFound);

        if (category.UserId != request.UserId)
            return Result.Failure(CategoryDomainErrors.AccessDenied);

        category.SoftDelete();
        await _categoryRepository.UpdateAsync(category, cancellationToken);

        return Result.Success();
    }
}
