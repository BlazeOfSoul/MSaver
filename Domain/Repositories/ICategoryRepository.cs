namespace MSaver.Domain.Repositories;

public interface ICategoryRepository
{
    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<Category> categories,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Category>> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default,
        Guid? excludeId = null);

    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
}