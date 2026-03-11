using server.Domain.Entities;

namespace server.Domain.Repositories;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetByUserIdWithCategoryAsync(Guid userId, CancellationToken cancellationToken = default);
}
