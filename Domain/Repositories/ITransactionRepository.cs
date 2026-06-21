using MSaver.Application.Features.Transactions.Get;

namespace MSaver.Domain.Repositories;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task RemoveAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetByTransferIdAsync(
        Guid transferId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetByTransferIdsWithDetailsAsync(
        IReadOnlyCollection<Guid> transferIds,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Transaction>> GetPagedWithDetailsAsync(
        TransactionListQuery query,
        CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, decimal>> SumByAccountIdsAsync(
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken = default);

    Task<decimal> GetBalanceBeforeAsync(
        Guid accountId,
        DateTime toExclusive,
        CancellationToken cancellationToken = default);

    Task<decimal> GetBalanceInPeriodAsync(
        Guid accountId,
        DateTime fromInclusive,
        DateTime toExclusive,
        CancellationToken cancellationToken = default);

    Task<TransactionPeriodBreakdown> GetBreakdownInPeriodAsync(
        Guid accountId,
        DateTime fromInclusive,
        DateTime toExclusive,
        CancellationToken cancellationToken = default);
}
