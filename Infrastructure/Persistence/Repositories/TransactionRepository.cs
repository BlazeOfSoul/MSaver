namespace MSaver.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(ApplicationDbContext dbContext) : ITransactionRepository
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

    public async Task<Transaction?> GetByIdWithCategoryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Include(t => t.TransactionTags)
            .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Transaction>> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Transaction>> GetByUserIdWithDetailsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Account!)
                .ThenInclude(a => a.Currency)
            .Include(t => t.TransactionTags)
                .ThenInclude(tt => tt.Tag)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, decimal>> SumByAccountIdsAsync(
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken = default)
    {
        if (accountIds.Count == 0)
            return new Dictionary<Guid, decimal>();

        return await _dbContext.Transactions
            .Where(t => accountIds.Contains(t.AccountId))
            .GroupBy(t => t.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                Total = g.Sum(x => x.Amount)
            })
            .ToDictionaryAsync(
                x => x.AccountId,
                x => x.Total,
                cancellationToken);
    }

    public async Task<decimal> GetBalanceAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var total = await _dbContext.Transactions
            .Where(t => t.AccountId == accountId)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken);

        return total ?? 0m;
    }

    public async Task<decimal> GetBalanceBeforeAsync(
        Guid accountId,
        DateTime toExclusive,
        CancellationToken cancellationToken = default)
    {
        var total = await _dbContext.Transactions
            .Where(t => t.AccountId == accountId && t.Date < toExclusive)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken);

        return total ?? 0m;
    }

    public async Task<decimal> GetBalanceInPeriodAsync(
        Guid accountId,
        DateTime fromInclusive,
        DateTime toExclusive,
        CancellationToken cancellationToken = default)
    {
        var total = await _dbContext.Transactions
            .Where(t => t.AccountId == accountId
                        && t.Date >= fromInclusive
                        && t.Date < toExclusive)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken);

        return total ?? 0m;
    }
}