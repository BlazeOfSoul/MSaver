using MSaver.Api.Contracts.Categories;
using MSaver.Application.Features.Categories.Common;
using MSaver.Application.Features.Categories.Create;
using MSaver.Application.Features.Categories.Get;
using MSaver.Application.Features.Categories.GetById;
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
        GetCategoriesRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        CategoryType? type = null;
        if (!string.IsNullOrWhiteSpace(request.Type) &&
            CategoryTypeHelper.TryParse(request.Type, out var parsedType))
        {
            type = parsedType;
        }

        var query = new CategoryListQuery
        {
            UserId = userId,
            Search = ListQueryHelper.NormalizeSearch(request.Search),
            SortBy = ListQueryHelper.NormalizeSortBy(request.SortBy, CategorySortFields.Name),
            SortDirection = ListQueryHelper.NormalizeSortDirection(request.SortDirection),
            Type = type,
            IncludeDeleted = request.IncludeDeleted,
            Page = request.Page,
            Size = request.Size
        };

        var pagedCategories = await _categoryRepository.GetPagedAsync(query, cancellationToken);

        var items = pagedCategories.Items
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
            Items = items,
            Page = pagedCategories.Page,
            Size = pagedCategories.Size,
            TotalCount = pagedCategories.TotalCount,
            TotalPages = pagedCategories.TotalPages,
            HasPreviousPage = pagedCategories.HasPreviousPage,
            HasNextPage = pagedCategories.HasNextPage
        });
    }

    public async Task<Result<GetCategoryByIdResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);

        if (category is null)
            return Result<GetCategoryByIdResponse>.Failure(CategoryDomainErrors.NotFound);

        if (category.UserId != userId)
            return Result<GetCategoryByIdResponse>.Failure(CategoryDomainErrors.AccessDenied);

        return Result<GetCategoryByIdResponse>.Success(new GetCategoryByIdResponse
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            Color = category.Color,
            IsDeleted = category.IsDeleted,
            IsSystem = category.IsSystem
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

        var category = Category.Create(
            userId,
            request.Name,
            request.Type,
            request.Color);

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(category.Id);
    }

    public async Task<Result<Guid>> UpdateAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
            return Result<Guid>.Failure(CategoryDomainErrors.NotFound);

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

        category.Update(
            request.Name,
            request.Color,
            request.Type);

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(category.Id);
    }

    public async Task<Result<Guid>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);

        if (category is null)
            return Result<Guid>.Failure(CategoryDomainErrors.NotFound);

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