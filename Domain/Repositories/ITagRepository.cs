namespace MSaver.Domain.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Tag?> GetByIdWithCategoriesAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Tag>> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default,
        Guid? excludeId = null);

    Task AddAsync(Tag tag, CancellationToken cancellationToken = default);

    Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
}