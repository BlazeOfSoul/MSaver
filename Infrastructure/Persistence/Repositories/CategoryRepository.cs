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

    public Task<Category?> GetTransferExpenseCategoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return Context.Categories
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.Type == CategoryType.TransferExpense,
                cancellationToken);
    }

    public Task<Category?> GetTransferIncomeCategoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return Context.Categories
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.Type == CategoryType.TransferIncome,
                cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default,
        Guid? excludeId = null)
    {
        var normalizedName = name.Trim();

        var query = Context.Categories
            .Where(x => x.UserId == userId && !x.IsDeleted && x.Name == normalizedName);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
