using MSaver.Application.Features.Transactions.Get;
using MSaver.Domain.Enums;

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
            .Include(t => t.Account)
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

    public async Task<Transaction?> GetByIdWithCategoryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetByTransferIdAsync(
        Guid transferId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.TransferId == transferId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetByTransferIdsWithDetailsAsync(
        IReadOnlyCollection<Guid> transferIds,
        CancellationToken cancellationToken = default)
    {
        if (transferIds.Count == 0)
            return [];

        return await _dbContext.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.TransferId.HasValue && transferIds.Contains(t.TransferId.Value))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Transaction>> GetPagedWithDetailsAsync(
        TransactionListQuery query,
        CancellationToken cancellationToken = default)
    {
        var fromDateUtc = UtcDateTime.Normalize(query.FromDate);
        var toDateUtc = UtcDateTime.Normalize(query.ToDate);

        IQueryable<Transaction> dbQuery = _dbContext.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Account!)
            .Where(t => t.UserId == query.UserId);

        if (query.AccountId.HasValue)
            dbQuery = dbQuery.Where(t => t.AccountId == query.AccountId.Value);

        if (query.CategoryId.HasValue)
            dbQuery = dbQuery.Where(t => t.CategoryId == query.CategoryId.Value);

        if (fromDateUtc.HasValue)
            dbQuery = dbQuery.Where(t => t.Date >= fromDateUtc.Value);

        if (toDateUtc.HasValue)
            dbQuery = dbQuery.Where(t => t.Date < toDateUtc.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            dbQuery = dbQuery.Where(t => t.Description.Contains(search));
        }

        dbQuery = ApplySorting(
            dbQuery,
            query.SortBy,
            query.SortDirection);

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync(cancellationToken);

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

    public async Task<decimal> GetBalanceBeforeAsync(
        Guid accountId,
        DateTime toExclusive,
        CancellationToken cancellationToken = default)
    {
        var toExclusiveUtc = UtcDateTime.Normalize(toExclusive);

        var total = await _dbContext.Transactions
            .Where(t => t.AccountId == accountId && t.Date < toExclusiveUtc)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken);

        return total ?? 0m;
    }

    public async Task<decimal> GetBalanceInPeriodAsync(
        Guid accountId,
        DateTime fromInclusive,
        DateTime toExclusive,
        CancellationToken cancellationToken = default)
    {
        var fromInclusiveUtc = UtcDateTime.Normalize(fromInclusive);
        var toExclusiveUtc = UtcDateTime.Normalize(toExclusive);

        var total = await _dbContext.Transactions
            .Where(t => t.AccountId == accountId
                        && t.Date >= fromInclusiveUtc
                        && t.Date < toExclusiveUtc)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken);

        return total ?? 0m;
    }

    public async Task<TransactionPeriodBreakdown> GetBreakdownInPeriodAsync(
        Guid accountId,
        DateTime fromInclusive,
        DateTime toExclusive,
        CancellationToken cancellationToken = default)
    {
        var fromInclusiveUtc = UtcDateTime.Normalize(fromInclusive);
        var toExclusiveUtc = UtcDateTime.Normalize(toExclusive);

        var totals = await _dbContext.Transactions
            .Where(t => t.AccountId == accountId
                        && t.Date >= fromInclusiveUtc
                        && t.Date < toExclusiveUtc)
            .GroupBy(t => t.Category!.Type)
            .Select(g => new
            {
                Type = g.Key,
                Total = g.Sum(t => t.Amount)
            })
            .ToDictionaryAsync(
                x => x.Type,
                x => x.Total,
                cancellationToken);

        return new TransactionPeriodBreakdown(
            Income: totals.GetValueOrDefault(CategoryType.Credit, 0m),
            Expense: totals.GetValueOrDefault(CategoryType.Debit, 0m),
            TransferIn: totals.GetValueOrDefault(CategoryType.TransferIncome, 0m),
            TransferOut: totals.GetValueOrDefault(CategoryType.TransferExpense, 0m));
    }

    private static IQueryable<Transaction> ApplySorting(
        IQueryable<Transaction> query,
        string? sortBy,
        string? sortDirection)
    {
        var normalizedSortBy = ListQueryHelper.NormalizeSortBy(
            sortBy,
            TransactionSortFields.Date);

        var normalizedSortDirection = ListQueryHelper.NormalizeSortDirection(sortDirection);

        return (normalizedSortBy, normalizedSortDirection) switch
        {
            (var field, var direction)
                when field.Equals(TransactionSortFields.Amount, StringComparison.OrdinalIgnoreCase)
                     && direction == ListQueryDefaults.SortAscending
                    => query
                        .OrderBy(t => t.Amount)
                        .ThenBy(t => t.Date)
                        .ThenBy(t => t.Id),

            (var field, var direction)
                when field.Equals(TransactionSortFields.Amount, StringComparison.OrdinalIgnoreCase)
                     && direction == ListQueryDefaults.SortDescending
                    => query
                        .OrderByDescending(t => t.Amount)
                        .ThenByDescending(t => t.Date)
                        .ThenByDescending(t => t.Id),

            (var field, var direction)
                when field.Equals(TransactionSortFields.Date, StringComparison.OrdinalIgnoreCase)
                     && direction == ListQueryDefaults.SortAscending
                    => query
                        .OrderBy(t => t.Date)
                        .ThenBy(t => t.Id),

            _ => query
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
        };
    }
}
