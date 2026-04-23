using MSaver.Application.Common.Models;
using MSaver.Application.Features.Accounts.Get;

namespace MSaver.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository(ApplicationDbContext context) : IAccountRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Account?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Account>> GetPagedAsync(
        AccountListQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Account> dbQuery = _context.Accounts
            .AsNoTracking()
            .Where(x => x.UserId == query.UserId);

        if (query.IsArchived.HasValue)
            dbQuery = dbQuery.Where(x => x.IsArchived == query.IsArchived.Value);

        if (!string.IsNullOrWhiteSpace(query.CurrencyCode))
            dbQuery = dbQuery.Where(x => x.CurrencyCode == query.CurrencyCode);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            dbQuery = dbQuery.Where(x => x.Name.Contains(search));
        }

        dbQuery = ApplySorting(dbQuery, query.SortBy, query.SortDirection);

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync(cancellationToken);

        return new PagedResult<Account>
        {
            Items = items,
            Page = query.Page,
            Size = query.Size,
            TotalCount = totalCount
        };
    }

    public async Task AddAsync(
        Account account,
        CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
    }

    public Task UpdateAsync(
        Account account,
        CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(account);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByNameAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default,
        Guid? excludeId = null)
    {
        var normalizedName = name.Trim();

        var query = _context.Accounts
            .Where(x => x.UserId == userId && x.Name == normalizedName);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public Task<bool> AnyAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _context.Accounts.AnyAsync(x => x.UserId == userId, cancellationToken);
    }

    private static IQueryable<Account> ApplySorting(
        IQueryable<Account> query,
        string? sortBy,
        string? sortDirection)
    {
        var normalizedSortBy = ListQueryHelper.NormalizeSortBy(
            sortBy,
            AccountSortFields.CreatedAt);

        var normalizedSortDirection = ListQueryHelper.NormalizeSortDirection(sortDirection);

        return (normalizedSortBy, normalizedSortDirection) switch
        {
            (var field, var direction)
                when field.Equals(AccountSortFields.Name, StringComparison.OrdinalIgnoreCase)
                     && direction == ListQueryDefaults.SortAscending
                    => query
                        .OrderBy(x => x.Name)
                        .ThenBy(x => x.Id),

            (var field, var direction)
                when field.Equals(AccountSortFields.Name, StringComparison.OrdinalIgnoreCase)
                     && direction == ListQueryDefaults.SortDescending
                    => query
                        .OrderByDescending(x => x.Name)
                        .ThenByDescending(x => x.Id),

            (var field, var direction)
                when field.Equals(AccountSortFields.CurrencyCode, StringComparison.OrdinalIgnoreCase)
                     && direction == ListQueryDefaults.SortAscending
                    => query
                        .OrderBy(x => x.CurrencyCode)
                        .ThenBy(x => x.Id),

            (var field, var direction)
                when field.Equals(AccountSortFields.CurrencyCode, StringComparison.OrdinalIgnoreCase)
                     && direction == ListQueryDefaults.SortDescending
                    => query
                        .OrderByDescending(x => x.CurrencyCode)
                        .ThenByDescending(x => x.Id),

            (var field, var direction)
                when field.Equals(AccountSortFields.CreatedAt, StringComparison.OrdinalIgnoreCase)
                     && direction == ListQueryDefaults.SortAscending
                    => query
                        .OrderBy(x => x.CreatedAtUtc)
                        .ThenBy(x => x.Id),

            _ => query
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.Id)
        };
    }
}