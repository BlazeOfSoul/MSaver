using MSaver.Domain.Entities;

namespace MSaver.Domain.Repositories;

public interface IBalanceRepository
{
    Task AddAsync(Balance balance, CancellationToken cancellationToken = default);

    Task<Balance?> GetByUserAndDateAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(Balance balance, CancellationToken cancellationToken = default);

    Task<Balance?> GetCurrentByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
