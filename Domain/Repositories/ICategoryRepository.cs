using MSaver.Application.Features.Categories.Get;

namespace MSaver.Domain.Repositories;

public interface ICategoryRepository
{
    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<Category> categories,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Category>> GetPagedAsync(
        CategoryListQuery query,
        CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Category>> GetByIdsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task<Category?> GetTransferExpenseCategoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Category?> GetTransferIncomeCategoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default,
        Guid? excludeId = null);

    Task<bool> HasTransactionsAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Category category,
        CancellationToken cancellationToken = default);
}
