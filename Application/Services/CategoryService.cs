using MSaver.Application.Features.Categories.Create;
using MSaver.Application.Features.Categories.Delete;
using MSaver.Application.Features.Categories.Get;
using MSaver.Application.Features.Categories.Update;

namespace MSaver.Application.Services;

public sealed class CategoryService(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<GetCategoriesResponse>> GetAsync(
        GetCategoriesRequest request,
        CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository
            .GetAsync(request.UserId, cancellationToken);

        var items = categories
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new CategoryItemResponse
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                Color = x.Color
            })
            .ToArray();

        return Result<GetCategoriesResponse>.Success(new GetCategoriesResponse
        {
            Items = items
        });
    }

    public async Task<Result<CreateCategoryResponse>> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var exists = await _categoryRepository.ExistsByNameAsync(
            request.UserId,
            request.Name,
            cancellationToken);

        if (exists)
            return Result<CreateCategoryResponse>.Failure(CategoryDomainErrors.NameAlreadyExists);

        try
        {
            var category = Domain.Entities.Category.Create(
                request.UserId,
                request.Name,
                request.Type,
                request.Color);

            await _categoryRepository.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CreateCategoryResponse>.Success(new CreateCategoryResponse(
                category.Id,
                category.Name,
                category.Type,
                category.Color));
        }
        catch (DomainException ex)
        {
            return Result<CreateCategoryResponse>.Failure(ex.Error);
        }
    }

    public async Task<Result<Guid>> UpdateAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
            return Result<Guid>.Failure(CategoryDomainErrors.CategoryNotFound);

        if (category.IsDeleted)
            return Result<Guid>.Failure(CategoryDomainErrors.CategoryDeleted);

        if (category.UserId != request.UserId)
            return Result<Guid>.Failure(CategoryDomainErrors.AccessDenied);

        var exists = await _categoryRepository.ExistsByNameAsync(
            request.UserId,
            request.Name,
            cancellationToken,
            request.Id);

        if (exists)
            return Result<Guid>.Failure(CategoryDomainErrors.NameAlreadyExists);

        try
        {
            category.Update(request.Name, request.Color, request.Type);

            await _categoryRepository.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(category.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Error);
        }
    }

    public async Task<Result<Guid>> DeleteAsync(
        DeleteCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
            return Result<Guid>.Failure(CategoryDomainErrors.CategoryNotFound);

        if (category.IsDeleted)
            return Result<Guid>.Failure(CategoryDomainErrors.CategoryDeleted);

        if (category.UserId != request.UserId)
            return Result<Guid>.Failure(CategoryDomainErrors.AccessDenied);

        category.SoftDelete();

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(category.Id);
    }
}