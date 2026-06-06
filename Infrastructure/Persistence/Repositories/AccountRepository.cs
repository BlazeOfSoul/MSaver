using MSaver.Application.Features.Accounts.Get;
using MSaver.Application.Features.Accounts.Specifications;

namespace MSaver.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository(ApplicationDbContext context)
    : EfRepositoryBase<Account>(context), IAccountRepository
{
    public Task<Account?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return FirstOrDefaultAsync(
            new AccountByIdSpecification(id),
            cancellationToken);
    }

    public async Task<PagedResult<Account>> GetPagedAsync(
        AccountListQuery query,
        CancellationToken cancellationToken = default)
    {
        var listSpecification = new AccountsListSpecification(query);
        var countSpecification = new AccountsCountSpecification(query);

        var totalCount = await CountAsync(countSpecification, cancellationToken);
        var items = await ListAsync(listSpecification, cancellationToken);

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
        await Context.Accounts.AddAsync(account, cancellationToken);
    }

    public Task UpdateAsync(
        Account account,
        CancellationToken cancellationToken = default)
    {
        Context.Accounts.Update(account);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByNameAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default,
        Guid? excludeId = null)
    {
        var normalizedName = name.Trim();

        var query = Context.Accounts
            .Where(x => x.UserId == userId && !x.IsArchived && x.Name == normalizedName);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public Task<bool> AnyAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return Context.Accounts.AnyAsync(x => x.UserId == userId, cancellationToken);
    }
}
