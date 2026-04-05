namespace MSaver.Domain.Repositories;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task RemoveAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Transaction>> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Transaction>> GetByUserIdWithCategoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<decimal> SumByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);
}