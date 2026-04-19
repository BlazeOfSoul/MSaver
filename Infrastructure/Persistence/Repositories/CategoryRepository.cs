using MSaver.Domain.Enums;

namespace MSaver.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(ApplicationDbContext dbContext) : ICategoryRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Categories.AddAsync(category, cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<Category> categories,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Categories.AddRangeAsync(categories, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Category>> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Category>> GetByIdsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        return await _dbContext.Categories
            .Where(x => x.UserId == userId && ids.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetTransferCategoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(
                x => x.UserId == userId &&
                     x.Type == CategoryType.Transfer &&
                     !x.IsDeleted,
                cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default,
        Guid? excludeId = null)
    {
        var query = _dbContext.Categories
            .Where(x => x.UserId == userId && x.Name == name && !x.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public Task UpdateAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Categories.Update(category);
        return Task.CompletedTask;
    }
}