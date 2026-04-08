using MSaver.Application.Features.Categories.Create;
using MSaver.Application.Features.Categories.Get;
using MSaver.Application.Features.Categories.Update;
using MSaver.Domain.Enums;

namespace MSaver.Application.Services;

public sealed class CategoryService(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<GetCategoriesResponse>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var categories = await _categoryRepository
            .GetAsync(userId, cancellationToken);

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

    public async Task<Result<Guid>> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var exists = await _categoryRepository.ExistsByNameAsync(
            userId,
            request.Name,
            cancellationToken);

        if (exists)
            return Result<Guid>.Failure(CategoryDomainErrors.NameAlreadyExists);

        try
        {
            var category = Domain.Entities.Category.Create(
                userId,
                request.Name,
                request.Type,
                request.Color);

            await _categoryRepository.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(category.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Error);
        }
    }

    public async Task<Result<Guid>> UpdateAsync(
    UpdateCategoryRequest request,
    CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
            return Result<Guid>.Failure(CategoryDomainErrors.CategoryNotFound);

        if (category.IsDeleted)
            return Result<Guid>.Failure(CategoryDomainErrors.CategoryDeleted);

        if (category.UserId != userId)
            return Result<Guid>.Failure(CategoryDomainErrors.AccessDenied);

        var exists = await _categoryRepository.ExistsByNameAsync(
            userId,
            request.Name,
            cancellationToken,
            request.Id);

        if (exists)
            return Result<Guid>.Failure(CategoryDomainErrors.NameAlreadyExists);

        if (!Enum.TryParse<CategoryType>(request.Type, true, out var type))
            return Result<Guid>.Failure(CategoryDomainErrors.InvalidCategoryType);

        try
        {
            category.Update(request.Name, request.Color, type);

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
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);

        if (category is null)
            return Result<Guid>.Failure(CategoryDomainErrors.CategoryNotFound);

        if (category.IsDeleted)
            return Result<Guid>.Failure(CategoryDomainErrors.CategoryDeleted);

        if (category.UserId != userId)
            return Result<Guid>.Failure(CategoryDomainErrors.AccessDenied);

        category.SoftDelete();

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(category.Id);
    }
}