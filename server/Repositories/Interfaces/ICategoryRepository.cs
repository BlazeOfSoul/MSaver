namespace server.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task AddRangeAsync(IEnumerable<Category> categories, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
}
