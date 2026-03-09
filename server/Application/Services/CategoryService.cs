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
        GetCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .Where(c => c.UserId == query.UserId && !c.IsDeleted)
            .Select(c => new CategoryResponse(
                c.Id,
                c.Name,
                c.Type,
                c.Color))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryDto> CreateCategoryAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Name = command.Name,
            Type = command.Type,
            Color = command.Color,
            IsDeleted = false
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            Color = category.Color
        };
    }

    public async Task<bool> UpdateCategoryAsync(
        UpdateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(
                c => c.Id == command.CategoryId && c.UserId == command.UserId && !c.IsDeleted,
                cancellationToken);

        if (category is null)
        {
            return false;
        }

        category.Name = command.Name;
        category.Color = command.Color;
        category.Type = command.Type;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(
        DeleteCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(
                c => c.Id == command.CategoryId && c.UserId == command.UserId,
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
