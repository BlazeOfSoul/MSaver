using MSaver.Application.Common.Models;
using MSaver.Application.Features.Accounts.Get;

namespace MSaver.Domain.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<Account>> GetPagedAsync(
        AccountListQuery query,
        CancellationToken cancellationToken = default);

    Task AddAsync(Account account, CancellationToken cancellationToken = default);

    Task UpdateAsync(Account account, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default,
        Guid? excludeId = null);

    Task<bool> AnyAsync(Guid userId, CancellationToken cancellationToken = default);
}