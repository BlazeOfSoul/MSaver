using Microsoft.EntityFrameworkCore;
using server.Application.Features.Categories.CreateCategory;
using server.Application.Features.Categories.DeleteCategory;
using server.Application.Features.Categories.GetCategories;
using server.Application.Features.Categories.UpdateCategory;
using server.Application.Services.Interfaces;
using server.Domain.Entities;
using server.Infrastructure.Persistence;

namespace server.Application.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(
        GetCategoriesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .Where(c => c.UserId == request.UserId && !c.IsDeleted)
            .Select(c => new CategoryResponse(
                c.Id,
                c.Name,
                c.Type,
                c.Color))
            .ToListAsync(cancellationToken);
    }

    public async Task<CreateCategoryResponse> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name,
            Type = request.Type,
            Color = request.Color,
            IsDeleted = false
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            Color = category.Color
        };
    }

    public async Task<bool> UpdateCategoryAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(
                c => c.Id == request.CategoryId &&
                     c.UserId == request.UserId &&
                     !c.IsDeleted,
                cancellationToken);

        if (category is null)
        {
            return false;
        }

        category.Name = request.Name;
        category.Color = request.Color;
        category.Type = request.Type;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(
        DeleteCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(
                c => c.Id == request.CategoryId &&
                     c.UserId == request.UserId,
                cancellationToken);

        if (category is null)
        {
            return false;
        }

        category.IsDeleted = true;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
