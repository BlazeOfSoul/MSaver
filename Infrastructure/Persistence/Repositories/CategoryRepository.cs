using MSaver.Application.Features.Categories.Get;
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

    public async Task<PagedResult<Category>> GetPagedAsync(
        CategoryListQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Category> dbQuery = _dbContext.Categories
            .AsNoTracking()
            .Where(x => x.UserId == query.UserId);

        if (query.Type.HasValue)
            dbQuery = dbQuery.Where(x => x.Type == query.Type.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            dbQuery = dbQuery.Where(x => x.Name.Contains(search));
        }

        dbQuery = ApplySorting(dbQuery, query.SortBy, query.SortDirection);

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync(cancellationToken);

        return new PagedResult<Category>
        {
            Items = items,
            Page = query.Page,
            Size = query.Size,
            TotalCount = totalCount
        };
    }

    public async Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
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

    public async Task<Category?> GetTransferExpenseCategoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(
                x => x.UserId == userId &&
                     x.Type == CategoryType.TransferExpense &&
                     !x.IsDeleted,
                cancellationToken);
    }

    public async Task<Category?> GetTransferIncomeCategoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(
                x => x.UserId == userId &&
                     x.Type == CategoryType.TransferIncome &&
                     !x.IsDeleted,
                cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default,
        Guid? excludeId = null)
    {
        var normalizedName = name.Trim();

        var query = _dbContext.Categories
            .Where(x => x.UserId == userId && x.Name == normalizedName && !x.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public Task UpdateAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Categories.Update(category);
        return Task.CompletedTask;
    }

    private static IQueryable<Category> ApplySorting(
        IQueryable<Category> query,
        string? sortBy,
        string? sortDirection)
    {
        var normalizedSortBy = ListQueryHelper.NormalizeSortBy(
            sortBy,
            CategorySortFields.Name);

        var normalizedSortDirection = ListQueryHelper.NormalizeSortDirection(sortDirection);

        return (normalizedSortBy, normalizedSortDirection) switch
        {
            (var field, var direction)
                when field.Equals(CategorySortFields.Type, StringComparison.OrdinalIgnoreCase)
                     && direction == ListQueryDefaults.SortAscending
                    => query
                        .OrderBy(x => x.Type)
                        .ThenBy(x => x.Name)
                        .ThenBy(x => x.Id),

            (var field, var direction)
                when field.Equals(CategorySortFields.Type, StringComparison.OrdinalIgnoreCase)
                     && direction == ListQueryDefaults.SortDescending
                    => query
                        .OrderByDescending(x => x.Type)
                        .ThenBy(x => x.Name)
                        .ThenBy(x => x.Id),

            (var field, var direction)
                when field.Equals(CategorySortFields.Name, StringComparison.OrdinalIgnoreCase)
                     && direction == ListQueryDefaults.SortDescending
                    => query
                        .OrderByDescending(x => x.Name)
                        .ThenByDescending(x => x.Id),

            _ => query
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
        };
    }
}