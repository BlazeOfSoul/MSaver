using server.Models;

namespace server.Repositories.Interfaces;

public interface IMonthlyBalanceRepository
{
    Task AddAsync(MonthlyBalance balance, CancellationToken cancellationToken = default);
    Task<MonthlyBalance?> GetByUserAndDateAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default);
    Task UpdateAsync(MonthlyBalance balance, CancellationToken cancellationToken = default);
    Task<MonthlyBalance?> GetCurrentByUserIdAsync(Guid userId);

}
