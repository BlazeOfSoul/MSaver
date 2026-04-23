using MSaver.Application.Common.Models;
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

    public async Task<PagedResult<Transaction>> GetPagedWithDetailsAsync(
        TransactionListQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Transaction> dbQuery = _dbContext.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Account!)
            .Where(t => t.UserId == query.UserId);

        if (query.AccountId.HasValue)
            dbQuery = dbQuery.Where(t => t.AccountId == query.AccountId.Value);

        if (query.CategoryId.HasValue)
            dbQuery = dbQuery.Where(t => t.CategoryId == query.CategoryId.Value);

        if (query.FromDate.HasValue)
            dbQuery = dbQuery.Where(t => t.Date >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            dbQuery = dbQuery.Where(t => t.Date < query.ToDate.Value);

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
                Total = g.Sum(t =>
                    t.Category!.Type == CategoryType.Debit ||
                    t.Category.Type == CategoryType.TransferExpense
                        ? -t.Amount
                        : t.Amount)
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