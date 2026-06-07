using MSaver.Application.Features.Categories.Get;
using MSaver.Application.Features.Categories.Specifications;
using MSaver.Domain.Enums;

namespace MSaver.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(ApplicationDbContext context) : EfRepositoryBase<Category>(context), ICategoryRepository
{
    public Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return FirstOrDefaultAsync(new CategoryByIdSpecification(id), cancellationToken);
    }

    public async Task<PagedResult<Category>> GetPagedAsync(
        CategoryListQuery query,
        CancellationToken cancellationToken = default)
    {
        var listSpecification = new CategoriesListSpecification(query);
        var countSpecification = new CategoriesCountSpecification(query);

        var totalCount = await CountAsync(countSpecification, cancellationToken);
        var items = await ListAsync(listSpecification, cancellationToken);

        return new PagedResult<Category>
        {
            Items = items,
            Page = query.Page,
            Size = query.Size,
            TotalCount = totalCount
        };
    }

    public async Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        await Context.Categories.AddAsync(category, cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<Category> categories,
        CancellationToken cancellationToken = default)
    {
        await Context.Categories.AddRangeAsync(categories, cancellationToken);
    }

    public Task UpdateAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        Context.Categories.Update(category);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyCollection<Category>> GetByIdsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        return await Context.Categories
            .Where(x => x.UserId == userId && ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<(Category? ExpenseCategory, Category? IncomeCategory)> GetTransferCategoriesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var categories = await Context.Categories
            .AsNoTracking()
            .Where(x => x.UserId == userId &&
                        (x.Type == CategoryType.TransferExpense ||
                         x.Type == CategoryType.TransferIncome))
            .ToListAsync(cancellationToken);

        return (
            categories.FirstOrDefault(x => x.Type == CategoryType.TransferExpense),
            categories.FirstOrDefault(x => x.Type == CategoryType.TransferIncome));
    }

    public async Task<bool> ExistsByNameAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default,
        Guid? excludeId = null)
    {
        var normalizedName = name.Trim();

        var query = Context.Categories
            .Where(x => x.UserId == userId && x.Name == normalizedName);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
