using MSaver.Application.Features.Transactions.Get;
using MSaver.Application.Features.Transactions.Specifications;

namespace MSaver.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(ApplicationDbContext dbContext)
    : EfRepositoryBase<Transaction>(dbContext), ITransactionRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Transactions.AddAsync(transaction, cancellationToken);
    }

    public Task UpdateAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Transactions.Update(transaction);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Transactions.Remove(transaction);
        return Task.CompletedTask;
    }

    public async Task<Transaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Transaction?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Transaction>> GetPagedWithDetailsAsync(
        TransactionListQuery query,
        CancellationToken cancellationToken = default)
    {
        var listSpecification = new TransactionsListSpecification(query);
        var countSpecification = new TransactionsCountSpecification(query);

        var totalCount = await CountAsync(countSpecification, cancellationToken);
        var items = await ListAsync(listSpecification, cancellationToken);

        return new PagedResult<Transaction>
        {
            Items = items,
            Page = query.Page,
            Size = query.Size,
            TotalCount = totalCount
        };
    }

    public async Task<Dictionary<Guid, decimal>> SumByAccountIdsAsync(
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken = default)
    {
        if (accountIds.Count == 0)
            return new Dictionary<Guid, decimal>();

        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => accountIds.Contains(t.AccountId))
            .GroupBy(t => t.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                Total = g.Sum(t => t.Amount)
            })
            .ToDictionaryAsync(
                x => x.AccountId,
                x => x.Total,
                cancellationToken);
    }

    public async Task<(decimal OpeningBalance, decimal PeriodChange)> GetBalanceForPeriodAsync(
        Guid accountId,
        DateTime fromInclusive,
        DateTime toExclusive,
        CancellationToken cancellationToken = default)
    {
        var totals = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId && t.Date < toExclusive)
            .GroupBy(t => t.AccountId)
            .Select(g => new
            {
                OpeningBalance = g
                    .Where(t => t.Date < fromInclusive)
                    .Sum(t => (decimal?)t.Amount) ?? 0m,
                PeriodChange = g
                    .Where(t => t.Date >= fromInclusive)
                    .Sum(t => (decimal?)t.Amount) ?? 0m
            })
            .FirstOrDefaultAsync(cancellationToken);

        return totals is null
            ? (0m, 0m)
            : (totals.OpeningBalance, totals.PeriodChange);
    }
}
